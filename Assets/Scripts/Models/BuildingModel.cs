using Core.Pools;
using Core.Utils;
using Models.Interfaces;
using UnityEngine;
using Zenject;

namespace Models
{
    public class BuildingModel : IBuildingModel
    {
        private const float MinWidth = 0.5f;
        private const int MaxWidth = 2;
        private const int BuildingHeight = 5;
        private const float StickShift = 0.1f;
        private readonly IPool<GameObject> _pool;
        private readonly GameObject _building;
        private GameObject _buildingWithPlayer;
        private GameObject _buildingToFollow;

        private Vector2 PlayerBuildingPosition 
            => new(_buildingWithPlayer.transform.position.x, 
                _buildingWithPlayer.transform.position.y + _buildingWithPlayer.transform.localScale.y * NumericConstants.Half  + NumericConstants.One);
        
        private Vector2 FollowBuildingPosition 
            => new(_buildingToFollow.transform.position.x, 
                _buildingToFollow.transform.position.y + _buildingToFollow.transform.localScale.y * NumericConstants.Half  + 1);

        public BuildingModel(IPool<GameObject> pool, [Inject(Id = "Building")] GameObject building)
        {
            _pool = pool;
            _building = building;
        }

        public void Initialize() => _pool.InitializePool(_building, Constants.DefaultPoolCount);

        public Vector2 GetPositionForPlayer(bool isDefault) => isDefault ? PlayerBuildingPosition : FollowBuildingPosition;

        public void SetNextBuildingPosition(Vector2 position)
        {
            if (_buildingWithPlayer == null)
            {
                _buildingWithPlayer = _pool.GetNextObject();
                _buildingWithPlayer.transform.position = position;
                _buildingWithPlayer.transform.localScale = CalculateNewScale();
                return;
            }
            
            if (_buildingToFollow == null)
            {
                _buildingToFollow = _pool.GetNextObject();
                _buildingToFollow.transform.position = position;
                _buildingToFollow.transform.localScale = CalculateNewScale();
                
                return;
            }
            _pool.ReturnObject(_buildingWithPlayer);
            _buildingWithPlayer = _buildingToFollow;
            _buildingToFollow = _pool.GetNextObject();
            _buildingToFollow.transform.position = position;
            _buildingToFollow.transform.localScale = CalculateNewScale();
        }

        private Vector3 CalculateNewScale() => new(Random.Range(MinWidth, MaxWidth), BuildingHeight, NumericConstants.One);

        public (float min, float max) GetNextBuildingPositionRange() 
            => (_buildingToFollow.transform.position.x - _buildingToFollow.transform.localScale.x * NumericConstants.Half, 
                _buildingToFollow.transform.position.x + _buildingToFollow.transform.localScale.x * NumericConstants.Half);

        public Vector2 CalculateStartPositionForStick() =>
            new(_buildingWithPlayer.transform.position.x + _buildingWithPlayer.transform.localScale.x * NumericConstants.Half - StickShift,
                _buildingWithPlayer.transform.position.y + _buildingWithPlayer.transform.localScale.y * NumericConstants.Half);
    }
}