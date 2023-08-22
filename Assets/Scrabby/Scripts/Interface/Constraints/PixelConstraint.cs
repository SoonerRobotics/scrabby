using System;
using UnityEngine;

namespace Scrabby.Interface.Constraints
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class PixelConstraint : MonoBehaviour
    {
        [Min(0)]
        public int pixels = 1;
        
        public Axis axis = Axis.X;
        public bool relativeToParent = true;
        public bool opposite = true;
        
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
                case Axis.Horizontal:
                    _localTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixels);
                    break;
                case Axis.Vertical:
                    _localTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixels);
                    break;
                case Axis.X:
                    if (relativeToParent)
                    {
                        var parentSize = _parentTransform.rect;
                        var newPosX = opposite ? pixels : parentSize.width - pixels - _localTransform.rect.width;
                        _localTransform.anchoredPosition = new Vector2(newPosX, _localTransform.anchoredPosition.y);
                    }
                    else
                    {
                        _localTransform.anchoredPosition = new Vector2(pixels, _localTransform.anchoredPosition.y);
                    }
                    break;
                case Axis.Y:
                    if (relativeToParent)
                    {
                        var parentSize = _parentTransform.rect;
                        var newPosY = opposite ? pixels : parentSize.height - pixels - _localTransform.rect.height;
                        _localTransform.anchoredPosition = new Vector2(_localTransform.anchoredPosition.x, newPosY);
                    }
                    else
                    {
                        _localTransform.anchoredPosition = new Vector2(_localTransform.anchoredPosition.x, pixels);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}