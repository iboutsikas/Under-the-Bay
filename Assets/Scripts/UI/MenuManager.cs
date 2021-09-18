using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTB.UI
{
    public class MenuManager : MonoBehaviourSingletonPersistent<MenuManager>
    {
        public MainMenuPanel MainMenu;

        public Button BurgerButton;

        public override void Awake()
        {
            base.Awake();

            BurgerButton.onClick.AddListener(On_BurgerButtonClicked);
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
    }
}
