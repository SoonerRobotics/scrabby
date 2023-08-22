using System;
using UnityEngine;

namespace Scrabby.Interface.Constraints
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class RatioConstraint : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float ratio = 1;
        
        public Axis axis = Axis.Horizontal;
        
        private RectTransform _localTransform;
        private RectTransform _parentTransform;

        private void Awake()
        {
            _localTransform = GetComponent<RectTransform>();
            _parentTransform = _localTransform.parent.GetComponent<RectTransform>();
        }

        private void Update()
        {
            var parentSize = _parentTransform.rect;
            switch (axis)
            {
                case Axis.Horizontal:
                    var newWidth = parentSize.width * ratio;
                    _localTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
                    break;
                case Axis.Vertical:
                    var newHeight = parentSize.height * ratio;
                    _localTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
                    break;
                case Axis.X:
                case Axis.Y:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}