using SimulationEngine.Source.Data.Geometry;
using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums;
using SimulationEngine.Source.Enums.EventTypes;
using SimulationEngine.Source.Enums.Logging;
using SimulationEngine.Source.Events.Payloads;
using SimulationEngine.Source.Factories;
using SimulationEngine.Source.Logistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulationEngine.Source.Systems
{
    internal static class SimulationSystem
    {

        private static string TroopId = "Troop";

        private static Random _rng = new Random();

        private static uint _currentId = 0;

        public static Game ActiveGame { get; private set; }

        public static HashSet<Cell> CheckForMatchPositions { get; set; }

        public static Dictionary<Cell,HashSet<Cell>> PositionsToActivate { get; private set; }

        public static HashSet<int> RefillColumnsIndexes { get; private set; }

        // Enemy special units that need gravity applied after an attack this turn
        public static List<uint> HitEnemySpecialUnits { get; private set; }

        public static int Seed
        {
            get;
            set
            {
                field = value;
                _rng = new Random(field);
            }
        }

        public static int RandomInt() => _rng.Next();
        public static uint NextId() => ++_currentId;

        public static void Init(int seed, uint currentId, Game game)
        {
            Seed = seed;
            _currentId = currentId;
            ActiveGame = game;
        }

        static SimulationSystem()
        {
            Seed = new Random().Next();
            ActiveGame = new(0, new Cell{ x=0,y=0});
            CheckForMatchPositions = new();
            PositionsToActivate = new();
            RefillColumnsIndexes = new();
            HitEnemySpecialUnits = new();
        }

        public static KeyValuePair<uint, Unit>? SpawnUnit(string unitId, Player owner)
        {
            Unit? unit = UnitFactory.GetUnit(unitId, owner);
            if (unit == null)
            {
                LogSystem.Log(ELogCategory.Debug, ELogLevel.Error, $"SimulationSystem.SpawnUnit Cannot spawn with id: {unitId} for player[{owner.Id}]");
                return null;
            }
            uint simId = NextId();
            return new KeyValuePair<uint, Unit>(simId, unit);
        }


        public static void CheckStateChain()
        {
            while(true){
                if (CheckForMatchPositions.Count > 0)
                {
                    CheckForMatches();
                    continue;
                }
                if (PositionsToActivate.Count > 0)
                {
                    MatchPositions();
                    continue;
                }

                if (RefillColumnsIndexes.Count > 0)
                {
                    RefillBoard();
                    continue;
                }

                break;
            }
            ApplyEnemyGravity();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Movement helpers (shared by Move command and ApplyEnemyGravity)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all board cells occupied by <paramref name="anchor"/> + <paramref name="shape"/>.
        /// </summary>
        private static IEnumerable<Cell> GetBoxCells(Cell anchor, Shape shape)
        {
            foreach (Cell offset in shape.GetOffsets())
                yield return anchor + offset;
        }

        /// <summary>
        /// Validates whether <paramref name="unitId"/> can move one step in <paramref name="direction"/>.
        /// <para>
        /// <paramref name="allowDisplace"/> = true  → any unit blocking the path is accepted when
        ///   CanDisplace AND that unit can itself step one cell in the same direction (recursively).
        ///   This allows a small unit to push a large one and supports push-chains.
        /// </para>
        /// <para>
        /// <paramref name="allowDisplace"/> = false → every target cell must be empty or already
        ///   owned by the same unit (gravity / enemy-advance mode).
        /// </para>
        /// </summary>
        public static bool CanUnitStep(Board board, Dictionary<uint, Unit> boardUnits,
            uint unitId, Cell direction, bool allowDisplace)
        {
            if (!boardUnits.TryGetValue(unitId, out Unit unit)) return false;

            Cell anchor = unit.Position;
            Shape shape = unit.Ocupation;

            HashSet<uint> displacedUnitIds = new();

            foreach (Cell offset in shape.GetOffsets())
            {
                Cell target = anchor + offset + direction;
                if (!board.IsInBounds(target)) return false;

                uint occupantId = board.Get(target);
                if (occupantId == 0 || occupantId == unitId) continue;

                if (!allowDisplace) return false;

                if (!boardUnits.TryGetValue(occupantId, out Unit occupant) || !occupant.CanDisplace)
                    return false;

                displacedUnitIds.Add(occupantId);
            }

            // Each displaced unit must itself be able to step in the same direction.
            // This check is recursive: a push-chain is valid only if every unit in the
            // chain has enough room.
            foreach (uint displacedId in displacedUnitIds)
            {
                if (!CanUnitStep(board, boardUnits, displacedId, direction, allowDisplace: true))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Executes one movement step for <paramref name="unitId"/> in <paramref name="direction"/>.
        /// Assumes <see cref="CanUnitStep"/> has already returned true.
        /// <para>
        /// Displaced units are moved first (furthest in the direction first so they clear
        /// space for units behind them), then the moving unit's cells are placed.
        /// </para>
        /// Fires <paramref name="movingUnitEvent"/> on the moving unit.
        /// Displaced units receive <see cref="EUnitEvent.Displace"/> via the recursive call.
        /// </summary>
        public static void ApplyUnitStep(Player player, uint unitId, Cell direction, EUnitEvent movingUnitEvent)
        {
            Board board = player.Board;
            Dictionary<uint, Unit> boardUnits = player.BoardUnits;

            if (!boardUnits.TryGetValue(unitId, out Unit unit)) return;

            Cell anchor = unit.Position;
            Shape shape = unit.Ocupation;

            // Collect every distinct unit that sits directly in this unit's path.
            HashSet<uint> displacedIds = new();
            foreach (Cell offset in shape.GetOffsets())
            {
                Cell target = anchor + offset + direction;
                uint occupantId = board.Get(target);
                if (occupantId != 0 && occupantId != unitId)
                    displacedIds.Add(occupantId);
            }

            // Move displaced units first — furthest in the movement direction goes first
            // so it clears space for units behind it (handles push-chains).
            List<uint> ordered = displacedIds
                .OrderByDescending(id =>
                {
                    boardUnits.TryGetValue(id, out Unit u);
                    return GetBoxCells(u.Position, u.Ocupation)
                        .Max(c => (c * direction).Magnitude());
                })
                .ToList();

            foreach (uint displacedId in ordered)
                ApplyUnitStep(player, displacedId, direction, EUnitEvent.Displace);

            // Now slide this unit one step: every cell moves into the adjacent cell in direction.
            // Because displaced units have already vacated those cells the target cells are free
            // (or are this unit's own trailing cells).
            List<Cell> offsets = shape.GetOffsets()
                .OrderByDescending(o => (o * direction).Magnitude())
                .ToList();

            foreach (Cell offset in offsets)
            {
                Cell currentCell = anchor + offset;
                Cell swapCell    = anchor + offset + direction;

                uint current  = board.Get(currentCell);
                uint swapWith = board.Get(swapCell);

                if (current == swapWith) continue;

                board.Set(currentCell, swapWith);
                board.Set(swapCell,    current);

                CheckForMatchPositions.Add(currentCell);
                CheckForMatchPositions.Add(swapCell);
            }

            unit.UnitEventBus.Raise(movingUnitEvent,
                new ValueChangedPayload<Cell>(anchor + direction, anchor));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Match detection
        // ─────────────────────────────────────────────────────────────────────

        private static void CheckForMatches()
        {
            Player player = ActiveGame.CurrentPlayer;
            Board board = player.Board;

            List<Cell> alreadyMatched = new();

            List<Cell> toCheck = new(CheckForMatchPositions);
            CheckForMatchPositions.Clear();


            Cell[] cardinals = {
                Cell.GetMoveDirection(EDirection.Up),
                Cell.GetMoveDirection(EDirection.Down),
                Cell.GetMoveDirection(EDirection.Left),
                Cell.GetMoveDirection(EDirection.Right)
            };


            // Each pair of perpendicular arms whose shared corner is a diagonal
            (int, int)[] diagPairs = { (0, 2), (0, 3), (1, 2), (1, 3) };

            foreach (Cell origin in toCheck)
            {
                if(alreadyMatched.Contains(origin)) continue;

                if (!board.IsInBounds(origin)) continue;

                uint originId = board.Get(origin);
                if (originId == 0) continue;
                if (!player.BoardUnits.TryGetValue(originId, out Unit originUnit)) continue;

                EColor color = originUnit.Color;

                // Scan each arm until a different color or empty cell is hit
                HashSet<Cell>[] arms = new HashSet<Cell>[4];
                for (int i = 0; i < 4; i++)
                {
                    arms[i] = new();
                    Cell cursor = origin + cardinals[i];
                    while (board.IsInBounds(cursor))
                    {
                        uint id = board.Get(cursor);
                        if (id == 0) break;
                        if (!player.BoardUnits.TryGetValue(id, out Unit u) || u.Color != color) break;
                        arms[i].Add(cursor);
                        cursor = cursor + cardinals[i];
                    }
                }

                KeyValuePair<Cell,HashSet<Cell>> group = new(origin, new());


                // Add the diagonal cell when both adjacent arms have at least one match
                foreach ((int a, int b) in diagPairs)
                {
                    if (arms[a].Count == 0 || arms[b].Count == 0) continue;
                    Cell diag = origin + cardinals[a] + cardinals[b];
                    if (!board.IsInBounds(diag)) continue;
                    uint diagId = board.Get(diag);
                    if (diagId == 0) continue;
                    if (!player.BoardUnits.TryGetValue(diagId, out Unit diagUnit) || diagUnit.Color != color) continue;
                    group.Value.Add(diag);
                    group.Value.Add(origin + cardinals[a]);
                    group.Value.Add(origin + cardinals[b]);
                }


                if (arms[0].Count + arms[1].Count > 1)
                {
                    foreach (Cell c in arms[0])
                        group.Value.Add(c);
                    foreach (Cell c in arms[1])
                        group.Value.Add(c);
                }
                if (arms[2].Count + arms[3].Count > 1)
                {
                    foreach (Cell c in arms[2])
                        group.Value.Add(c);
                    foreach (Cell c in arms[3])
                        group.Value.Add(c);
                }


                if (group.Value.Count < 2) continue;

                alreadyMatched.Add(group.Key);
                alreadyMatched.AddRange(group.Value);

                PositionsToActivate.Add(group.Key, group.Value);
            }
        }

        private static void MatchPositions()
        {
            Player player = ActiveGame.CurrentPlayer;
            Board board = player.Board;

            HashSet<uint> activated = new HashSet<uint>();
            Dictionary<Cell, HashSet<Cell>> groups = new(PositionsToActivate);
            PositionsToActivate.Clear();

            foreach (KeyValuePair<Cell, HashSet<Cell>> group in groups)
            {
                uint id;
                Unit unit;
                foreach (Cell cell in group.Value)
                {
                    id = board.Get(cell);
                    if (id == 0 || activated.Contains(id)) continue;
                    if (!player.BoardUnits.TryGetValue(id, out unit)) continue;

                    unit.UnitEventBus.Raise(EUnitEvent.TryActivate, new EventPayload());
                    activated.Add(id);
                }

                id = board.Get(group.Key);
                if (id == 0 || activated.Contains(id)) continue;
                if (!player.BoardUnits.TryGetValue(id, out unit)) continue;

                if (group.Value.Count > 2)
                {
                    unit.UnitEventBus.Raise(EUnitEvent.Promote, new ValuePayload<int>(group.Value.Count - 3));
                }else
                {
                    unit.UnitEventBus.Raise(EUnitEvent.Promote, new EventPayload());
                }
                activated.Add(id);
            }
        }

        public static void SetupBoard(Player owner)
        {
            Board board = owner.Board;
            for(int i=0; i< board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {
                    Cell pos = new Cell { x = i, y = j };
                    KeyValuePair<uint, Unit>? newTroop = SpawnUnit(TroopId, owner);
                    if (newTroop == null) continue;
                    owner.BoardUnits.Add(newTroop.Value.Key, newTroop.Value.Value);
                    newTroop.Value.Value.Position = pos;
                    board.Set(pos, newTroop.Value.Key);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Gravity: board refill — units fall toward y=0 (top of board)
        // ─────────────────────────────────────────────────────────────────────

        private static void RefillBoard()
        {
            Player player = ActiveGame.CurrentPlayer;
            Board board = player.Board;

            HashSet<int> columns = new(RefillColumnsIndexes);
            RefillColumnsIndexes.Clear();

            foreach (int col in columns)
            {
                // Collect every falling unit in this column, top-to-bottom (y=0 first).
                // Clear them from the board as we go so we can repack cleanly.
                List<(uint id, Cell prevPos, Unit unit)> units = new();
                for (int y = 0; y < board.Height; y++)
                {
                    Cell pos = new Cell { x = col, y = y };
                    uint id = board.Get(pos);
                    if (id == 0) continue;

                    if (!player.BoardUnits.TryGetValue(id, out Unit fallingUnit)) continue;

                    if (!fallingUnit.CanFall) continue;

                    units.Add((id, pos, fallingUnit));
                    board.Set(pos, 0);
                }

                // Repack from y=0 downward: surviving units pack at the top (y=0 side),
                // new spawns fill whatever remains at the bottom (high y side).
                for (int y = 0; y < board.Height; y++)
                {
                    Cell pos = new Cell { x = col, y = y };
                    if (board.Get(pos) != 0) continue;   // non-falling unit occupying this cell

                    CheckForMatchPositions.Add(pos);

                    if (units.Count > 0)
                    {
                        (uint id, Cell prevPos, Unit unit) = units[0];
                        board.Set(pos, id);
                        unit.UnitEventBus.Raise(EUnitEvent.Fall, new ValueChangedPayload<Cell>(pos, prevPos));
                        units.RemoveAt(0);
                        continue;
                    }

                    // No survivors left — spawn a fresh troop at this bottom position
                    KeyValuePair<uint, Unit>? newTroop = SpawnUnit(TroopId, player);
                    if (newTroop == null) continue;
                    player.BoardUnits.Add(newTroop.Value.Key, newTroop.Value.Value);
                    newTroop.Value.Value.Position = pos;
                    board.Set(pos, newTroop.Value.Key);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Enemy gravity — enemy special units advance toward y=0 (EDirection.Down)
        // ─────────────────────────────────────────────────────────────────────

        private static void ApplyEnemyGravity()
        {
            Player enemy = ActiveGame.OtherPlayer;
            Board board = enemy.Board;

            List<uint> units = new(HitEnemySpecialUnits);
            HitEnemySpecialUnits.Clear();

            Cell down = Cell.GetMoveDirection(EDirection.Down); // {y: -1} → toward y=0

            units.RemoveAll(id => !enemy.BoardUnits.ContainsKey(id));

            // Process units closest to y=0 first so they don't block units behind them.
            units = units.OrderBy(u =>
            {
                enemy.BoardUnits.TryGetValue(u, out Unit unit);
                // Find the topmost offset (lowest y) of this unit's shape
                int minOffsetY = unit.Ocupation.GetOffsets()
                    .OrderBy(o => o.y)
                    .First().y;
                return unit.Position.y + minOffsetY;
            }).ToList();

            foreach (uint unitId in units)
            {
                if (!CanUnitStep(board, enemy.BoardUnits, unitId, down, allowDisplace: false))
                    continue;

                ApplyUnitStep(enemy, unitId, down, EUnitEvent.Displace);
            }
        }
    }
}
