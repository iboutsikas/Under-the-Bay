using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTB.UI
{
    public class MenuManager : MonoBehaviourSingletonPersistent<MenuManager>
    {
        public MainMenu MainMenu;
        public SystemMenu SystemMenu;

        public Button BurgerButton;

        public override void Awake()
        {
            base.Awake();

            BurgerButton.onClick.AddListener(On_BurgerButtonClicked);

            Debug.Assert(MainMenu != null);
            Debug.Assert(SystemMenu != null);

            MainMenu.RegisterManager(this);
            SystemMenu.RegisterManager(this);
        }

        private void OnDestroy()
        {
            BurgerButton.onClick.RemoveListener(On_BurgerButtonClicked);
        }

        private void On_BurgerButtonClicked()
        {
            if (MainMenu.IsOpen())
                MainMenu.PopOut();
            else
                MainMenu.PopIn();
        }

        public void OpenSystemMenu()
        {
            if (MainMenu.IsOpen())
                MainMenu.PopOut();

            SystemMenu.PopIn();
        }
    }
}
