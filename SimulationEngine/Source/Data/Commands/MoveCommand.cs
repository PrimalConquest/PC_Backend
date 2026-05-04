using SharedUtils.Source.Logging;
using SimulationEngine.Source.Data.Geometry;
using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums;
using SimulationEngine.Source.Factories.Commands.CommandInfos;
using SimulationEngine.Source.Interfaces;
using SimulationEngine.Source.Logistic;
using SimulationEngine.Source.Systems;

namespace SimulationEngine.Source.Data.Commands
{
    public class MoveCommand : IGameCommand
    {
        Player     _player;
        Unit?      _movingUnit;
        EDirection _direction;

        public MoveCommand(Player player, MoveCommandInfo info) : this(player, info.Position, info.Direction) { }

        protected MoveCommand(Player player, Cell pos, EDirection direction)
        {
            _player     = player;
            _movingUnit = _player.Board.Get(pos);
            _direction  = direction;

            if (_movingUnit == null)
                LogSystem.Log(ELogCategory.Debug, ELogLevel.Error,
                    $"MoveCommand - no unit at [{pos.x},{pos.y}]");
        }

        public bool CanExecute()
        {
            if (SimulationSystem.ActiveGame.CurrentPlayer != _player) return false;
            if (_player.CurrentMoves < 1) return false;
            return _movingUnit != null;
        }

        public void Execute()
        {
            if (_movingUnit == null) return;

            var before = _player.Board.SnapshotPositions();

            MoveStack? moveStack = SimulationSystem.GattherMoveStack(
                _player.Board, _movingUnit, _direction);

            if (moveStack == null)
            {
                _player.Board.RollbackPositions(before);
                SimulationSystem.CheckStateChain();
                return;
            }

            // Cost exactly 1 move regardless of how many units were pushed.
            _player.CurrentMoves -= 1;

            // Seed match detection for every cell whose occupant changed.
            var after = _player.Board.SnapshotPositions();
            foreach (var kv in after)
            {
                if (!before.TryGetValue(kv.Key, out Cell prev) || prev != kv.Value)
                    SimulationSystem.CheckForMatchPositions.Add(kv.Value);
            }

            SimulationSystem.CheckStateChain();
        }
    }
}
