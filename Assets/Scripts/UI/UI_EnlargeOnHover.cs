using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Script used for making stuff larger when hovered
    /// </summary>
    public class UIEnlargeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The object's default scale
        /// </summary>
        private Vector3 defaultSize;
        /// <summary>
        /// The desired scale when hovered
        /// </summary>
        [SerializeField] private float hoveredSizeMultiplier;
        /// <summary>
        /// The target object that needs to be scaled when hovered
        /// </summary>
        [SerializeField] private GameObject target;

        public void Start()
        {
            defaultSize = target.transform.localScale;
        }

        //Enlarge the icon
        public void OnPointerEnter(PointerEventData eventData)
        {
            target.transform.localScale = defaultSize * hoveredSizeMultiplier;
        }
        
        //Make icon smaller
        public void OnPointerExit(PointerEventData eventData)
        {
            target.transform.localScale = defaultSize;
        }
    }
}
