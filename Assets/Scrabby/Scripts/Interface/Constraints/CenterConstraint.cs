using System;
using UnityEngine;

namespace Scrabby.Interface.Constraints
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class CenterConstraint : MonoBehaviour
    {
        public Axis axis = Axis.X;

        private RectTransform _localTransform;
        private RectTransform _parentTransform;

        private void Awake()
        {
            _localTransform = GetComponent<RectTransform>();
            _parentTransform = _localTransform.parent.GetComponent<RectTransform>();
            
            _localTransform.pivot = new Vector2(0, 0);
            _localTransform.anchorMin = new Vector2(0, 0);
            _localTransform.anchorMax = new Vector2(0, 0);
        }

        private void Update()
        {
            switch (axis)
            {
                case Axis.X:
                    var parentWidth = _parentTransform.rect;
                    var localWidth = _localTransform.rect;
                    var newX = (parentWidth.width - localWidth.width) / 2;
                    _localTransform.anchoredPosition = new Vector2(newX, _localTransform.anchoredPosition.y);
                    break;
                case Axis.Y:
                    var parentHeight = _parentTransform.rect;
                    var localHeight = _localTransform.rect;
                    var newY = (parentHeight.height - localHeight.height) / 2;
                    _localTransform.anchoredPosition = new Vector2(_localTransform.anchoredPosition.x, newY);
                    break;
                case Axis.Horizontal:
                case Axis.Vertical:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}