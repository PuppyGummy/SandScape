using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TabController : MonoBehaviour
    {
        [SerializeField]
        private List<Button> tabButtons = new List<Button>();

        [SerializeField] private GameObject tabContainer;
        
        [SerializeField]
        private List<GameObject> tabs = new List<GameObject>();

        private GameObject currentTab;
        
        // Start is called before the first frame update
        void Start()
        {
            AddButtons();
            AddTabs();

            currentTab = tabs[0];
        }

        /// <summary>
        /// Adds all buttons to list, and sets up listener events for each
        /// </summary>
        private void AddButtons()
        {
            foreach (var button in gameObject.GetComponentsInChildren<Button>())
            {
                tabButtons.Add(button);
                button.onClick.AddListener(() => ButtonPressed(button.gameObject));
            }
        }

        /// <summary>
        /// Registers all tabs by getting child components from the tab container.
        /// </summary>
        private void AddTabs()
        {
            foreach (var tab in tabContainer.GetComponentsInChildren<Transform>(true))
            {
                //Filter out any tab's child components, by making sure we're only getting root children of the tab container
                if(tab.parent.name != "Tabs")
                    continue;
                
                tabs.Add(tab.gameObject);
            }
        }

        /// <summary>
        /// Listener event for when the button is pressed - use this to call appropriate logic for tab switching
        /// </summary>
        /// <param name="pressedButton">The gameobject for the button that was pressed</param>
        private void ButtonPressed(GameObject pressedButton)
        {
            Debug.Log("Pressed " + pressedButton.gameObject.name);
            SwitchTabs(pressedButton.gameObject);
        }

        private void SwitchTabs(GameObject pressedButton)
        {
            //Setup vars
            GameObject desiredTab;
            Button buttonComponent = pressedButton.GetComponent<Button>();

            //Assign the desired tab based on clicked button
            desiredTab = tabs[tabButtons.IndexOf(buttonComponent)];
            
            //Early return if tab shouldn't change
            if(desiredTab == currentTab)
                return;
            
            //Disable current tab
            currentTab.gameObject.SetActive(false);
            
            //Enable new tab and save current tab
            desiredTab.SetActive(true);
            currentTab = desiredTab;
        }
    }
}
