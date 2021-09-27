using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTB.UI
{
    [DefaultExecutionOrder(-1)]
    public class MenuManager : MonoBehaviourSingletonPersistent<MenuManager>
    {
        private Dictionary<MenuType, MenuPanel> m_Menus;
        private MenuPanel m_CurrentMenu = null;


        public Button BurgerButton;

        public override void Awake()
        {
            base.Awake();

            m_Menus = new Dictionary<MenuType, MenuPanel>();
            BurgerButton.onClick.AddListener(On_BurgerButtonClicked);

            Debug.Assert(BurgerButton != null);
        }

        private void OnDestroy()
        {
            BurgerButton.onClick.RemoveListener(On_BurgerButtonClicked);
        }

        private void On_BurgerButtonClicked()
        {
            RequestOpenMenu(MenuType.MAIN_MENU);
        }

        public void RegisterMenu(MenuPanel menu)
        {
            Debug.Assert(menu.MenuType != MenuType.NONE, 
                "A concrete panel should always have a type other than NONE");
            
            if (m_Menus.ContainsKey(menu.MenuType))
            {
                Debug.LogWarning($"There is already a menu of type {menu.MenuType} and we tried to register another!");
                return;
            }

            m_Menus.Add(menu.MenuType, menu);
        }

        public void RequestOpenMenu(MenuType type)
        {
            Debug.Assert(type != MenuType.NONE,
                            "A concrete panel should always have a type other than NONE");

            if (m_CurrentMenu != null && m_CurrentMenu.MenuType == type)
                return;

            MenuPanel temp = null;

            if (m_Menus.TryGetValue(type, out temp))
            {
                if (m_CurrentMenu != null)
                    m_CurrentMenu.PopOut();

                temp.PopIn();
                m_CurrentMenu = temp;
            }
        }

        public void RequestClose(MenuPanel menu)
        {
            Debug.Assert(menu.MenuType != MenuType.NONE,
                "A concrete panel should always have a type other than NONE");

            if (m_CurrentMenu == menu)
            {
                m_CurrentMenu.PopOut();
                m_CurrentMenu = null;
            }
            else
            {
                Debug.LogWarning("Tried to close menu that is not open");
            }
        }
    }
}
