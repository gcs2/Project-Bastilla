// ============================================================================
// RPGPlatform.Systems.Combat - Positioning Systems
// Grid-based and free-form positioning options
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Grid-based positioning system
    /// </summary>
    public class GridPositioningSystem : IPositioningSystem
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _origin;
        private readonly HashSet<Vector2Int> _blockedCells;
        private readonly Dictionary<Vector2Int, ICombatant> _occupiedCells;
        
        public bool IsGridBased => true;
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        
        public GridPositioningSystem(int width, int height, float cellSize = 1f, Vector3 origin = default)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;
            _blockedCells = new HashSet<Vector2Int>();
            _occupiedCells = new Dictionary<Vector2Int, ICombatant>();
        }
        
        public float GetDistance(ICombatant a, ICombatant b)
        {
            // Manhattan distance for grid
            var posA = a.Position.GridPosition;
            var posB = b.Position.GridPosition;
            return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y);
        }
        
        public bool IsInRange(ICombatant attacker, ICombatant target, int range)
        {
            return GetDistance(attacker, target) <= range;
        }
        
        public bool IsValidPosition(CombatPosition position)
        {
            var grid = position.GridPosition;
            
            // Check bounds
            if (grid.x < 0 || grid.x >= _width || grid.y < 0 || grid.y >= _height)
                return false;
            
            // Check if blocked
            if (_blockedCells.Contains(grid))
                return false;
            
            // Check if occupied
            if (_occupiedCells.ContainsKey(grid))
                return false;
            
            return true;
        }
        
        public List<ICombatant> GetCombatantsInArea(CombatPosition center, int radius, List<ICombatant> allCombatants)
        {
            var result = new List<ICombatant>();
            var centerGrid = center.GridPosition;
            
            foreach (var combatant in allCombatants)
            {
                var pos = combatant.Position.GridPosition;
                int distance = Mathf.Abs(pos.x - centerGrid.x) + Mathf.Abs(pos.y - centerGrid.y);
                
                if (distance <= radius)
                {
                    result.Add(combatant);
                }
            }
            
            return result;
        }
        
        public List<CombatPosition> GetValidMovePositions(ICombatant combatant, int movementRange)
        {
            var result = new List<CombatPosition>();
            var start = combatant.Position.GridPosition;
            
            // BFS to find all reachable positions
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<(Vector2Int pos, int dist)>();
            queue.Enqueue((start, 0));
            visited.Add(start);
            
            var directions = new[] 
            { 
                new Vector2Int(0, 1), 
                new Vector2Int(0, -1), 
                new Vector2Int(1, 0), 
                new Vector2Int(-1, 0) 
            };
            
            while (queue.Count > 0)
            {
                var (pos, dist) = queue.Dequeue();
                
                if (dist > 0) // Don't include starting position
                {
                    var combatPos = CombatPosition.FromGrid(pos.x, pos.y);
                    combatPos.WorldPosition = GridToWorld(pos);
                    result.Add(combatPos);
                }
                
                if (dist < movementRange)
                {
                    foreach (var dir in directions)
                    {
                        var next = pos + dir;
                        if (!visited.Contains(next) && IsValidPosition(CombatPosition.FromGrid(next.x, next.y)))
                        {
                            visited.Add(next);
                            queue.Enqueue((next, dist + 1));
                        }
                    }
                }
            }
            
            return result;
        }
        
        public bool TryMove(ICombatant combatant, CombatPosition newPosition)
        {
            if (!IsValidPosition(newPosition))
                return false;
            MoveCombatant(combatant, newPosition);
            return true;
        }

        public void MoveCombatant(ICombatant combatant, CombatPosition newPosition)
        {
            var oldGrid = combatant.Position.GridPosition;
            var newGrid = newPosition.GridPosition;
            
            // Update occupancy
            _occupiedCells.Remove(oldGrid);
            _occupiedCells[newGrid] = combatant;
            
            // Update combatant position
            combatant.Position = new CombatPosition
            {
                GridPosition = newGrid,
                WorldPosition = GridToWorld(newGrid)
            };
        }
        
        /// <summary>
        /// Place a combatant on the grid
        /// </summary>
        public bool PlaceCombatant(ICombatant combatant, Vector2Int gridPos)
        {
            var position = CombatPosition.FromGrid(gridPos.x, gridPos.y);
            if (!IsValidPosition(position))
                return false;
            
            _occupiedCells[gridPos] = combatant;
            combatant.Position = new CombatPosition
            {
                GridPosition = gridPos,
                WorldPosition = GridToWorld(gridPos)
            };
            
            return true;
        }
        
        /// <summary>
        /// Remove a combatant from the grid
        /// </summary>
        public void RemoveCombatant(ICombatant combatant)
        {
            var grid = combatant.Position.GridPosition;
            _occupiedCells.Remove(grid);
        }
        
        /// <summary>
        /// Block a cell (obstacle, wall, etc.)
        /// </summary>
        public void BlockCell(Vector2Int cell)
        {
            _blockedCells.Add(cell);
        }
        
        /// <summary>
        /// Unblock a cell
        /// </summary>
        public void UnblockCell(Vector2Int cell)
        {
            _blockedCells.Remove(cell);
        }
        
        /// <summary>
        /// Convert grid position to world position
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return _origin + new Vector3(
                gridPos.x * _cellSize + _cellSize / 2,
                0,
                gridPos.y * _cellSize + _cellSize / 2
            );
        }
        
        /// <summary>
        /// Convert world position to grid position
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            var local = worldPos - _origin;
            return new Vector2Int(
                Mathf.FloorToInt(local.x / _cellSize),
                Mathf.FloorToInt(local.z / _cellSize)
            );
        }
        
        /// <summary>
        /// Get the combatant at a specific grid position
        /// </summary>
        public ICombatant GetCombatantAt(Vector2Int gridPos)
        {
            return _occupiedCells.TryGetValue(gridPos, out var combatant) ? combatant : null;
        }
    }
    
    /// <summary>
    /// Free-form (non-grid) positioning system
    /// </summary>
    public class FreeFormPositioningSystem : IPositioningSystem
    {
        private readonly float _arenaRadius;
        private readonly Vector3 _center;
        private readonly List<Bounds> _obstacles;
        
        public bool IsGridBased => false;
        public float ArenaRadius => _arenaRadius;
        
        public FreeFormPositioningSystem(float arenaRadius = 50f, Vector3 center = default)
        {
            _arenaRadius = arenaRadius;
            _center = center;
            _obstacles = new List<Bounds>();
        }
        
        public float GetDistance(ICombatant a, ICombatant b)
        {
            return Vector3.Distance(a.Position.WorldPosition, b.Position.WorldPosition);
        }
        
        public bool IsInRange(ICombatant attacker, ICombatant target, int range)
        {
            // Convert range (grid units) to world units
            float worldRange = range * 1.5f; // 1.5 units per range point
            return GetDistance(attacker, target) <= worldRange;
        }
        
        public bool IsValidPosition(CombatPosition position)
        {
            var pos = position.WorldPosition;
            
            // Check arena bounds
            if (Vector3.Distance(pos, _center) > _arenaRadius)
                return false;
            
            // Check obstacles
            foreach (var obstacle in _obstacles)
            {
                if (obstacle.Contains(pos))
                    return false;
            }
            
            return true;
        }
        
        public List<ICombatant> GetCombatantsInArea(CombatPosition center, int radius, List<ICombatant> allCombatants)
        {
            var result = new List<ICombatant>();
            float worldRadius = radius * 1.5f;
            
            foreach (var combatant in allCombatants)
            {
                float dist = Vector3.Distance(combatant.Position.WorldPosition, center.WorldPosition);
                if (dist <= worldRadius)
                {
                    result.Add(combatant);
                }
            }
            
            return result;
        }
        
        public List<CombatPosition> GetValidMovePositions(ICombatant combatant, int movementRange)
        {
            // For free-form, we return sample positions in a circle
            var result = new List<CombatPosition>();
            var start = combatant.Position.WorldPosition;
            float worldRange = movementRange * 1.5f;
            
            // Sample positions at regular intervals
            int samples = 16;
            for (int r = 1; r <= 3; r++)
            {
                float radius = worldRange * r / 3f;
                for (int i = 0; i < samples; i++)
                {
                    float angle = (2 * Mathf.PI * i) / samples;
                    var pos = start + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0,
                        Mathf.Sin(angle) * radius
                    );
                    
                    var combatPos = CombatPosition.FromWorld(pos);
                    if (IsValidPosition(combatPos))
                    {
                        result.Add(combatPos);
                    }
                }
            }
            
            return result;
        }
        
        public bool TryMove(ICombatant combatant, CombatPosition newPosition)
        {
            if (!IsValidPosition(newPosition))
                return false;
            MoveCombatant(combatant, newPosition);
            return true;
        }

        public void MoveCombatant(ICombatant combatant, CombatPosition newPosition)
        {
            combatant.Position = newPosition;
        }
        
        /// <summary>
        /// Add an obstacle to the arena
        /// </summary>
        public void AddObstacle(Bounds obstacle)
        {
            _obstacles.Add(obstacle);
        }
        
        /// <summary>
        /// Remove an obstacle
        /// </summary>
        public void RemoveObstacle(Bounds obstacle)
        {
            _obstacles.Remove(obstacle);
        }
    }
    
    /// <summary>
    /// Factory for creating positioning systems
    /// </summary>
    public static class PositioningSystemFactory
    {
        public static IPositioningSystem CreateGrid(int width, int height, float cellSize = 1f)
        {
            return new GridPositioningSystem(width, height, cellSize);
        }
        
        public static IPositioningSystem CreateFreeForm(float arenaRadius = 50f)
        {
            return new FreeFormPositioningSystem(arenaRadius);
        }
    }
}
