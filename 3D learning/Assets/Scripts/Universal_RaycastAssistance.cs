using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Universal_RaycastAssistance
{
    private static Universal_RaycastAssistance savedInstance;
    public static Universal_RaycastAssistance instance { get { return savedInstance == null? savedInstance = new Universal_RaycastAssistance(): savedInstance; } private set { } }
    public float[] GetBetweenValues(float _from, float _to, int _amount)
    {
        Debug.Log("WORKIUNG");
        List<float> finalList = new List<float>();

        var LineDistance = _to - _from;
        var PointDistance = LineDistance / _amount;

        for (float i = _from; i < _to; i += PointDistance)
        {
            finalList.Add(i);
        }
        return finalList.ToArray();
    }
    public bool IsItProperHeight(Vector3 _feetPos, Vector3 _direction, float _maxheight, LayerMask _layerMask, out RaycastHit HeightHit, float _distanceFromDirection = .35f, float PlayerHeight = 2f)
    {
        HeightHit = new RaycastHit();
        bool _fronthitted = Physics.Raycast(_feetPos + new Vector3(0f, _maxheight, 0f), _direction, out RaycastHit _frontHit, _distanceFromDirection, _layerMask);
        if (!_fronthitted)
        {
            var _downHitted = Physics.Raycast(_feetPos + new Vector3(0f, PlayerHeight, 0f) + _direction.normalized * _distanceFromDirection, Vector3.down, out RaycastHit _downHit, Mathf.Infinity, _layerMask);
            if (_downHitted)
            {
                if (_downHit.point.y - _feetPos.y > 0.1f && _downHit.point.y < _feetPos.y + _maxheight)
                {
                    HeightHit = _downHit;
                    return true;
                }
            }
        }
        return false;
    }
    public void IsItProperHeightGizmos(Vector3 _feetPos, Vector3 _direction, float _maxheight, LayerMask _layerMask, float _distanceFromDirection = .35f, float _playerHeight = 2f)
    {
        bool _fronthitted = Physics.Raycast(_feetPos + new Vector3(0f, _maxheight, 0f), _direction, out RaycastHit _frontHit, _distanceFromDirection, _layerMask);
        Gizmos.color = Color.white;
        DrawRaycastGizmo(_feetPos + new Vector3(0f, _maxheight, 0f),_direction,_distanceFromDirection);
        if (!_fronthitted)
        {
            var _downHitted = Physics.Raycast(_feetPos + new Vector3(0f, _playerHeight, 0f) + _direction.normalized * _distanceFromDirection, Vector3.down, out RaycastHit _downHit, Mathf.Infinity, _layerMask);
            DrawRaycastGizmo(_feetPos + new Vector3(0f, _playerHeight, 0f) + _direction.normalized * _distanceFromDirection, Vector3.down, 50f);
            if (_downHitted)
            {
                if (_downHit.point.y - _feetPos.y > 0.1f && _downHit.point.y < _feetPos.y + _maxheight)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(_downHit.point, .1f);
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
    public bool RaycastHitFromToY(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount, LayerMask _layerMask,out RaycastHit _lowestHit, out RaycastHit _heighestHit, float DotmaxAngle = 0f)
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

        Debug.Log(Vector2.Dot(Vector2.up, _heighestHitGround.normal));
        if (Vector2.Dot(Vector2.up, _heighestHitGround.normal) < DotmaxAngle) return false;
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
    public void RaycastHitFromToYGizmos(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount, LayerMask _layerMask,Color _HitColor,Color _HeighestHitColor,Color _LowestHitColor, out RaycastHit _lowestHit, out RaycastHit _heighestHit, float DotmaxAngle = 0f)
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
        if (!_hittedHeighestHitGround) return;

        Debug.Log(Vector2.Dot(Vector2.up, _heighestHitGround.normal));
        if (Vector2.Dot(Vector2.up, _heighestHitGround.normal) < DotmaxAngle) return;
        if(_HitPositions.Contains(_lowestHitOrigin)) _HitPositions.RemoveAt(_HitPositions.IndexOf(_lowestHitOrigin));
        if (_HitPositions.Contains(_heighestHitOrigin)) _HitPositions.RemoveAt(_HitPositions.IndexOf(_heighestHitOrigin));
        if (_NoneHitPositions.Contains(_lowestHitOrigin)) _NoneHitPositions.RemoveAt(_NoneHitPositions.IndexOf(_lowestHitOrigin));
        if (_NoneHitPositions.Contains(_heighestHitOrigin)) _NoneHitPositions.RemoveAt(_NoneHitPositions.IndexOf(_heighestHitOrigin));
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

    public void RaycastHitFromToZGizmos(Vector3 _startPosition, Vector3 _RaycastsDirection, float _distance, float _from, float _to, int _amount, LayerMask _layerMask, Color _HitColor, Color _HeighestHitColor, Color _LowestHitColor, out RaycastHit _lowestHit, out RaycastHit _heighestHit, float DotmaxAngle = 0f)
    {
        List<Vector3> _HitPositions = new List<Vector3>();
        List<Vector3> _NoneHitPositions = new List<Vector3>();
        float _lastLowestZHitPos = float.MaxValue;
        RaycastHit _LastlowestZhit = new RaycastHit();
        float _LastHeighestZHitPos = float.MinValue;
        RaycastHit _LastHeighestZHit = new RaycastHit();
        foreach (var _zValue in GetBetweenValues(_from, _to, _amount))
        {
            Vector3 origin = new Vector3(_startPosition.x, _startPosition.y, _zValue);
            bool hitted = Physics.Raycast(origin, _RaycastsDirection, out RaycastHit hit, _distance, _layerMask);
            if (hitted)
            {
                _HitPositions.Add(origin);
                if (hit.point.z < _lastLowestZHitPos)
                {
                    _lastLowestZHitPos = hit.point.z;
                    _LastlowestZhit = hit;
                }

                if (hit.point.z > _LastHeighestZHitPos)
                {
                    _LastHeighestZHitPos = hit.point.z;
                    _LastHeighestZHit = hit;
                }

            }
            else
            {
                _NoneHitPositions.Add(origin);
            }

        }
        var _lowestHitOrigin = new Vector3(_startPosition.x, _startPosition.y, _lastLowestZHitPos);
        var _heighestHitOrigin = new Vector3(_startPosition.x, _startPosition.y, _LastHeighestZHitPos);
        _lowestHit = _LastlowestZhit;
        _heighestHit = _LastHeighestZHit;
        //Debug.Log(CalculateNormal(_lowestHit.point, _heighestHit.point));
        var _hittedHeighestHitGround = Physics.Raycast(_heighestHit.point + new Vector3(0f, 0.05f, 0f), Vector3.down, out RaycastHit _heighestHitGround, Mathf.Infinity, _layerMask);
        if (!_hittedHeighestHitGround) return;

        Debug.Log(Vector2.Dot(Vector2.up, _heighestHitGround.normal));
        if (Vector2.Dot(Vector2.up, _heighestHitGround.normal) < DotmaxAngle) return;
        if (_HitPositions.Contains(_lowestHitOrigin)) _HitPositions.RemoveAt(_HitPositions.IndexOf(_lowestHitOrigin));
        if (_HitPositions.Contains(_heighestHitOrigin)) _HitPositions.RemoveAt(_HitPositions.IndexOf(_heighestHitOrigin));
        if (_NoneHitPositions.Contains(_lowestHitOrigin)) _NoneHitPositions.RemoveAt(_NoneHitPositions.IndexOf(_lowestHitOrigin));
        if (_NoneHitPositions.Contains(_heighestHitOrigin)) _NoneHitPositions.RemoveAt(_NoneHitPositions.IndexOf(_heighestHitOrigin));
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
