using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Universal_RaycastAssistance
{
    private static Universal_RaycastAssistance savedInstance;
    public static Universal_RaycastAssistance instance { get { return savedInstance == null? savedInstance = new Universal_RaycastAssistance(): savedInstance; } private set { } }
    public float[] GetBetweenValues(float _from, float _to, int _amount, float multiplayer = 1)
    {
        //Debug.Log("WORKIUNG");
        List<float> finalList = new List<float>();

        var LineDistance = _to - _from;
        var PointDistance = LineDistance / _amount;

        for (float i = _from; i < _to; i += PointDistance)
        {
            finalList.Add(i * multiplayer);
        }
        return finalList.ToArray();
    }
    public bool IsItProperHeight(Vector3 _feetPos, Vector3 RaycastPos, Vector3 _direction, float _maxheight, float _maxLengthFromPlayerToObstacle, LayerMask _layerMask, float _dotMaxAngle, out RaycastHit HeightHit, float _distanceFromDirection = .35f, float _playerHeight = 2f)
    {
        HeightHit = new RaycastHit();
        var _raycastPosWHeight = RaycastPos + new Vector3(0f, _playerHeight, 0f);
        var _playerRaycastStartPosWHeight = _feetPos + _maxheight * Vector3.up + Vector3.down * 0.02f;
        var _maxLengthFromPlayerToObstacleWOffset = _maxLengthFromPlayerToObstacle + .1f;
        bool _fronthitted = Physics.Raycast(_playerRaycastStartPosWHeight, _direction, out RaycastHit _frontHit, _maxLengthFromPlayerToObstacleWOffset, _layerMask);
        if (!_fronthitted)
        {
            var _downHitted = Physics.Raycast(_raycastPosWHeight + _direction.normalized * _distanceFromDirection, Vector3.down, out RaycastHit _downHit, Mathf.Infinity, _layerMask);
            if (_downHitted)
            {
                Physics.Raycast(_feetPos + new Vector3(0f, _playerHeight, 0f), Vector3.down, out RaycastHit _feetHit, Mathf.Infinity, _layerMask);
                if (_downHit.point.y < _feetHit.point.y + _maxheight && Vector2.Dot(Vector2.up, _downHit.normal) > _dotMaxAngle)
                {
                    HeightHit = _downHit;
                    return true;
                }
            }
        }
        return false;
    }
    public void IsItProperHeightGizmos(Vector3 _feetPos, Vector3 RaycastPos, Vector3 _direction, float _maxheight, float _maxLengthFromPlayerToObstacle, LayerMask _layerMask, float _distanceFromDirection = .35f, float _playerHeight = 2f)
    {

        var _raycastPosWHeight = RaycastPos + new Vector3(0f, _playerHeight, 0f);
        var _playerRaycastStartPosWHeight = _feetPos + _maxheight * Vector3.up + Vector3.down * 0.02f;
        var _maxLengthFromPlayerToObstacleWOffset = _maxLengthFromPlayerToObstacle + .1f;
        bool _fronthitted = Physics.Raycast(_playerRaycastStartPosWHeight, _direction, out RaycastHit _frontHit, _maxLengthFromPlayerToObstacleWOffset, _layerMask);
        Gizmos.color = (!_fronthitted) ? Color.white : Color.red;
        Gizmos.DrawCube(_playerRaycastStartPosWHeight + _direction * _maxLengthFromPlayerToObstacleWOffset, new Vector3(.05f,.05f,.05f));
        DrawRaycastGizmo(_playerRaycastStartPosWHeight, _direction, _maxLengthFromPlayerToObstacleWOffset);
        Gizmos.color = Color.white;
        if (!_fronthitted)
            if (!_fronthitted)
        {
            var _downHitted = Physics.Raycast(_raycastPosWHeight + _direction.normalized * _distanceFromDirection, Vector3.down, out RaycastHit _downHit, Mathf.Infinity, _layerMask);
            DrawRaycastGizmo(_raycastPosWHeight + _direction.normalized * _distanceFromDirection, Vector3.down, 50f);
            if (_downHitted)
            {
                Physics.Raycast(_feetPos + new Vector3(0f, _playerHeight, 0f), Vector3.down, out RaycastHit _feetHit, Mathf.Infinity, _layerMask);
                if (/*_downHit.point.y - _feetHit.point.y > 0.1f &&*/ _downHit.point.y < _feetHit.point.y + _maxheight)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(_downHit.point, .05f);
                    //return true;
                }
            }
        }
        //return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="DotmaxAngle">closer to 1 means closer to flat gorund, closer to 0 means closer to something heigh</param>
    public bool RaycastHitFromToY(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount, LayerMask _layerMask,out RaycastHit _lowestHit, out RaycastHit _heighestHit)
    {
        float _lastLowestYHitPos = float.MaxValue;
        RaycastHit _LastlowestYhit = new RaycastHit();
        float _LastHeighestYHitPos = float.MinValue;
        RaycastHit _LastHeighestYHit = new RaycastHit();
        foreach (var _yValue in GetBetweenValues(_from,_to,_amount))
        {
            Vector3 origin = new Vector3(_startPosition.x,_yValue,_startPosition.z);
            bool hitted = Physics.Raycast(origin, _RaycastsDirection, out RaycastHit hit, _distance, _layerMask);
            if (!hitted) continue;
            if (hit.point.y < _lastLowestYHitPos)
            {
                _lastLowestYHitPos = hit.point.y;
                _LastlowestYhit = hit;
            }
            if(hit.point.y > _LastHeighestYHitPos)
            {
                _LastHeighestYHitPos = hit.point.y;
                _LastHeighestYHit = hit;
            }
        }
        _lowestHit = new RaycastHit();
        _heighestHit = new RaycastHit();
        var _hittedHeighestHitGround = Physics.Raycast(_LastHeighestYHit.point + new Vector3(0f, 0.05f, 0f), Vector3.down, out RaycastHit _heighestHitGround, Mathf.Infinity, _layerMask);
        if (!_hittedHeighestHitGround) return false;
        _lowestHit = _LastlowestYhit;
        _heighestHit = _LastHeighestYHit;
        return true;
    }
    public void RaycastHitFromToYGizmos(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount)
    {
        foreach (var _yValue in GetBetweenValues(_from, _to, _amount))
        {
            Vector3 origin = new Vector3(_startPosition.x, _yValue, _startPosition.z);
            DrawRaycastGizmo(origin, _RaycastsDirection, _distance);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="DotmaxAngle">closer to 1 means closer to flat gorund, closer to 0 means closer to something heigh</param>
    public bool RaycastHitFromToYGizmos(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount, LayerMask _layerMask,Color _HitColor,Color _HeighestHitColor,Color _LowestHitColor, out RaycastHit _lowestHit, out RaycastHit _heighestHit)
    {
        List<Vector3> _HitPositions = new List<Vector3>();
        List<Vector3> _NoneHitPositions = new List<Vector3>();
        float _lastLowestYHitPos = float.MaxValue;
        RaycastHit _LastlowestYhit = new RaycastHit();
        float _LastHeighestYHitPos = float.MinValue;
        RaycastHit _LastHeighestYHit = new RaycastHit();
        foreach (var _yValue in GetBetweenValues(_from, _to, _amount))
        {
            Vector3 origin = new Vector3(_startPosition.x, _yValue, _startPosition.z);
            bool hitted = Physics.Raycast(origin, _RaycastsDirection, out RaycastHit hit, _distance, _layerMask);
            if (hitted)
            {
                _HitPositions.Add(origin);
                if (hit.point.y < _lastLowestYHitPos)
                {
                    _lastLowestYHitPos = hit.point.y;
                    _LastlowestYhit = hit;
                }
                
                if (hit.point.y > _LastHeighestYHitPos)
                {
                    _LastHeighestYHitPos = hit.point.y;
                    _LastHeighestYHit = hit;
                }
                
            }
            else
            {
                _NoneHitPositions.Add(origin);
            }

        }
        var _lowestHitOrigin = new Vector3(_startPosition.x, _lastLowestYHitPos, _startPosition.z);
        var _heighestHitOrigin = new Vector3(_startPosition.x, _LastHeighestYHitPos, _startPosition.z);
        _lowestHit = _LastlowestYhit;
        _heighestHit = _LastHeighestYHit;
        //Debug.Log(CalculateNormal(_lowestHit.point, _heighestHit.point));
        var _hittedHeighestHitGround = Physics.Raycast(_heighestHit.point + new Vector3(0f,0.05f,0f), Vector3.down, out RaycastHit _heighestHitGround, Mathf.Infinity, _layerMask);
        if (!_hittedHeighestHitGround) return false;

       //Debug.Log(Vector2.Dot(Vector2.up, _heighestHitGround.normal));
        //if (Vector2.Dot(Vector2.up, _heighestHitGround.normal) < DotmaxAngle) return false;
        foreach (var pos in _HitPositions)
        {
            Gizmos.color = _HitColor;
            DrawRaycastGizmo(pos, _RaycastsDirection, _distance);
        }
        foreach (var pos in _NoneHitPositions)
        {
            Gizmos.color = Color.white;
            DrawRaycastGizmo(pos, _RaycastsDirection, _distance);
        }
        Gizmos.color = _LowestHitColor;
        DrawRaycastGizmo(_lowestHitOrigin, _RaycastsDirection, _distance);
        Gizmos.color = _HeighestHitColor;
        DrawRaycastGizmo(_heighestHitOrigin, _RaycastsDirection, _distance);
        return true;
    }

    public void RaycastHitFromToZGizmos(Vector3 _startPosition, Vector3 _feetPos, Vector3 _RaycastsDirection, Vector3 offset, Vector3 facingDirection, float _distance, float _to, int _amount, LayerMask _layerMask, Color _HitColor, Color _HeighestHitColor, Color _LowestHitColor, out RaycastHit _lowestHit, out RaycastHit _heighestHit)
    {
        List<Vector3> _HitPositions = new List<Vector3>();
        List<Vector3> _NoneHitPositions = new List<Vector3>();
        float _lastLowestZHitPos = float.MaxValue;
        Vector3 _lastLowestZHitOrigin = new Vector3();
        RaycastHit _LastlowestZhit = new RaycastHit();
        float _LastHeighestZHitPos = float.MinValue;
        Vector3 _LastHeighestZHitOrigin = new Vector3();
        RaycastHit _LastHeighestZHit = new RaycastHit();
        foreach (var _zValue in GetBetweenValues(0, _to, _amount))
        {
           // Debug.Log(_zValue);
            Vector3 origin = facingDirection * _zValue + new Vector3(_startPosition.x, _startPosition.y, _startPosition.z) + offset - new Vector3(0f, _startPosition.y - _feetPos.y, 0f);
            bool hitted = Physics.Raycast(origin, _RaycastsDirection, out RaycastHit hit, _distance, _layerMask);
            if (hitted && hit.point.y - _startPosition.y > 0.01f)
            {
                _HitPositions.Add(origin);
                var posDiff = Mathf.Abs(hit.point.z - _startPosition.z);
                if (posDiff < _lastLowestZHitPos)
                {
                    _lastLowestZHitPos = posDiff;
                    _lastLowestZHitOrigin = origin;
                    _LastlowestZhit = hit;
                }

                if (posDiff > _LastHeighestZHitPos)
                {
                    _LastHeighestZHitPos = posDiff;
                    _LastHeighestZHitOrigin = origin;
                    _LastHeighestZHit = hit;
                }
            }
            else
            {
                _NoneHitPositions.Add(origin);
            }

        }
        var _lowestHitOrigin = _lastLowestZHitOrigin;
        var _heighestHitOrigin = _LastHeighestZHitOrigin;
        _lowestHit = _LastlowestZhit;
        _heighestHit = _LastHeighestZHit;
        //Debug.Log(CalculateNormal(_lowestHit.point, _heighestHit.point));
        //var _hittedHeighestHitGround = Physics.Raycast(_heighestHit.point + new Vector3(0f, 0.05f, 0f), Vector3.down, out RaycastHit _heighestHitGround, Mathf.Infinity, _layerMask);
        //if (!_hittedHeighestHitGround) return;

        //Debug.Log(Vector2.Dot(Vector2.up, _heighestHitGround.normal));
        //if (Vector2.Dot(Vector2.up, _heighestHitGround.normal) < DotmaxAngle) return;
        foreach (var pos in _HitPositions)
        {
            Gizmos.color = _HitColor;
            DrawRaycastGizmo(pos, _RaycastsDirection, _distance);
        }
        foreach (var pos in _NoneHitPositions)
        {
            Gizmos.color = Color.white;
            DrawRaycastGizmo(pos, _RaycastsDirection, _distance);
        }
        Gizmos.color = _LowestHitColor;
        DrawRaycastGizmo(_lowestHitOrigin, _RaycastsDirection, _distance);
        Gizmos.color = _HeighestHitColor;
        DrawRaycastGizmo(_heighestHitOrigin, _RaycastsDirection, _distance);
    }
    public bool RaycastHitFromToZ(Vector3 _startPosition, Vector3 _feetPos, Vector3 _RaycastsDirection, Vector3 offset, Vector3 facingDirection, float _distance, float _to, int _amount, LayerMask _layerMask,out RaycastHit _lowestHit, out RaycastHit _heighestHit)
    {
        float _lastLowestZHitPos = float.MaxValue;
        RaycastHit _LastlowestZhit = new RaycastHit();
        float _LastHeighestZHitPos = float.MinValue;
        RaycastHit _LastHeighestZHit = new RaycastHit();
        bool startHitted = false;
        foreach (var _zValue in GetBetweenValues(0, _to, _amount))
        {
            //Debug.Log(_zValue);
            Vector3 origin = facingDirection * _zValue + new Vector3(_startPosition.x, _startPosition.y, _startPosition.z) + offset - new Vector3(0f, _startPosition.y - _feetPos.y, 0f);
            bool hitted = Physics.Raycast(origin, _RaycastsDirection, out RaycastHit hit, _distance, _layerMask);
            if (!hitted) continue;
            startHitted = true;
            if (startHitted && !hitted) break;
            var posDiff = Mathf.Abs(hit.point.z - _startPosition.z);
            if (posDiff < _lastLowestZHitPos)
            {
                _lastLowestZHitPos = posDiff;
                _LastlowestZhit = hit;
            }

            if (posDiff > _LastHeighestZHitPos)
            {
                _LastHeighestZHitPos = posDiff;
                _LastHeighestZHit = hit;
            }
        }
        _lowestHit = _LastlowestZhit;
        _heighestHit = _LastHeighestZHit;
        //Debug.Log($"startHitted:{startHitted} && {_heighestHit.point.y - _startPosition.y} > 0.01f");
        return startHitted && _heighestHit.point.y - _startPosition.y > 0.01f;
    }
    private Vector2 CalculateNormal(Vector2 v1,Vector2 v2)
    {
        var dx = v2.x - v1.x;
        var dy = v2.y - v1.y;
        return new Vector2(-dy, dx);
    }
    public float CalculateNomal(Vector2 vector1, Vector2 vector2)
    {
        return Vector3.Cross(vector1,vector2).magnitude;
    }
    public void DrawRaycastGizmo(Vector3 origin, Vector3 direciton, float Distance)
    {
        Gizmos.DrawLine(origin, origin + direciton * Distance);
    }
    public Vector3[] GetDirectionsAroundDirectionInYAngle(Vector3 _direction,float _maxAngle, int _amount)
    {
        var _betweenValues = GetBetweenValues(0f, _maxAngle,_amount);
        var _betweenValuesNegative = GetBetweenValues(0f, _maxAngle, _amount, -1f);
        var _betweenValuesMixed = _MixLists(_betweenValues, _betweenValuesNegative);
        Vector3[] vector3s = new Vector3[_betweenValuesMixed.Length];
        for (int i = 0; i < _betweenValuesMixed.Length; i++)
        {
            var angle = _betweenValuesMixed[i];
            vector3s[i] = Quaternion.Euler(0f, angle, 0f) * _direction;
        }
        return vector3s.ToArray();
    }
    private T[] _MixLists<T>(T[] _arrayA, T[] _arrayB)
    {
        T[] combinedArrayAB = new T[_arrayA.Length + _arrayB.Length];
        for (int i = 0; i < combinedArrayAB.Length; i++)
        {
            combinedArrayAB[i] = (i % 2 == 0) ? _arrayA[Mathf.FloorToInt(i / 2)]: _arrayB[Mathf.FloorToInt(i / 2)];
        }
        return combinedArrayAB;
    }
    public Vector3[] UniformPointsOnYAxis(Vector3 _from, float _to, int _amount, int _ignoreFromStartIndexes = 0)
    {
        var _betweenValues = GetBetweenValues(0, _to, _amount);
        Vector3[] _points = new Vector3[_betweenValues.Length - _ignoreFromStartIndexes];
        for (int i = 0; i < _betweenValues.Length; i++)
        {
            if (i < _ignoreFromStartIndexes) continue;
            var _betweenValue = _betweenValues[i];
            _points[i - _ignoreFromStartIndexes] = _from + Vector3.up * _betweenValue;
        }
        return _points;
    }
    public Vector3[] UniformPointsOnSphere(float numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();
        float i = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2 / numberOfPoints;
        float halfOffset = 0.5f * offset;
        int currPoint = 0;
        for (; currPoint < numberOfPoints; currPoint++)
        {
            float y = currPoint * offset - 1 + halfOffset;
            float r = Mathf.Sqrt(1 - y * y);
            float phi = currPoint * i;
            Vector3 point = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
            if (!points.Contains(point)) points.Add(point);
        }
        return points.ToArray();
    }
    public Vector3[] UniformPointsOnSphere(float numberOfPoints, Vector3 LookDir, float MinDotProduct = .5f)
    {
        List<Vector3> points = new List<Vector3>();
        float i = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2 / numberOfPoints;
        float halfOffset = 0.5f * offset;
        int currPoint = 0;
        for (; currPoint < numberOfPoints; currPoint++)
        {
            float y = currPoint * offset - 1 + halfOffset;
            float r = Mathf.Sqrt(1 - y * y);
            float phi = currPoint * i;
            Vector3 point = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
            if (!points.Contains(point))
            {
                if (Vector3.Dot(LookDir.normalized, point.normalized) > MinDotProduct)
                {
                    points.Add(point);
                }
            }
        }
        return points.ToArray();
    }
}
