using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_LoginHandler : MonoBehaviour
    {
        [SerializeField]
        protected TM_UI_LunchHandler lunchHandler;

        [Space(10)]
        [SerializeField]
        protected TM_UI_ToastMessageHandler toastMessage;

        [Space(10)]
        [SerializeField]
        protected Button loginButton;
        [SerializeField]
        protected Button createAccountButton;

        [Space(10)]
        [SerializeField]
        protected TMP_InputField loginPanelIDInputField;
        [SerializeField]
        protected TMP_InputField loginPanelPasswordInputField;

        [Space(10)]
        [SerializeField]
        protected TMP_InputField createAccountPanelIDInputField;
        [SerializeField]
        protected TMP_InputField createAccountPanelPasswordInputField;
        [SerializeField]
        protected TMP_InputField createAccountNameInputField;

        [Space(10)]
        [SerializeField]
        protected UnityEvent onLoginedEvent;

        protected SelectionCircleChain selectionChain;

        protected virtual void Awake()
        {
            TM_TCPClient.OnResponseLogin += OnResponseLogin;
            TM_TCPClient.OnResponseCreateAccount += OnResponseCreateAccount;

            selectionChain = new SelectionCircleChain(loginPanelIDInputField,
                                                      loginPanelPasswordInputField,
                                                      loginButton,

                                                      createAccountPanelIDInputField,
                                                      createAccountPanelPasswordInputField,
                                                      createAccountNameInputField,
                                                      createAccountButton);
        }

        protected virtual void OnDestroy()
        {
            TM_TCPClient.OnResponseCreateAccount -= OnResponseCreateAccount;
            TM_TCPClient.OnResponseLogin -= OnResponseLogin;
        }

        protected virtual void Start()
        {
            loginButton.onClick.AddListener(OnClickLoginButton);
            void OnClickLoginButton()
            {
                string userID = loginPanelIDInputField.text;
                string password = loginPanelPasswordInputField.text;

                if (string.IsNullOrEmpty(userID) == true)
                {
                    toastMessage.Show("아이디를 입력하세요");
                }
                else if (string.IsNullOrEmpty(password) == true)
                {
                    toastMessage.Show("비밀번호를 입력하세요");
                }
                else
                {
                    toastMessage.Show("로딩중...", 60f, true);
                    TM_TCPClient.Instance.RequestLogin(userID, password.ToEncryptAES(TM_DBHandler.ENCRYPTION_KEY));
                }
            }

            createAccountButton.onClick.AddListener(OnClickCreateAccountButton);
            void OnClickCreateAccountButton()
            {
                string userName = createAccountNameInputField.text;
                string userID = createAccountPanelIDInputField.text;
                string userPassword = createAccountPanelPasswordInputField.text;
             
                if (string.IsNullOrEmpty(userID) == true)
                {
                    toastMessage.Show("아이디를 입력하세요");
                }
                else if (userID.ToLower().Contains("admin") == true)
                {
                    toastMessage.Show("admin 이 포함된 아이디는 쓸 수 없습니다.");
                }
                else if (string.IsNullOrEmpty(userPassword) == true)
                {
                    toastMessage.Show("비밀번호를 입력하세요");
                }
                else if (string.IsNullOrEmpty(userName) == true)
                {
                    toastMessage.Show("이름을 입력하세요");
                }
                else if (userName.ToLower().Contains("admin") == true)
                {
                    toastMessage.Show("admin 이 포함된 이름은 쓸 수 없습니다.");
                }
                else
                {
                    TM_TCPClient.Instance.RequestCreateAccount(userName,
                                                               userID,
                                                               userPassword.ToEncryptAES(TM_DBHandler.ENCRYPTION_KEY),
                                                               DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
                }
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) == true)
            {
                selectionChain.MoveNext();
            }

            if (Input.GetKeyDown(KeyCode.Return) == true)
            {
                if (selectionChain != null)
                {
                    if (selectionChain.CurrentObj == loginPanelPasswordInputField.gameObject)
                    {
                        loginButton.onClick.Invoke();
                    }
                }
            }
        }

        protected virtual void OnResponseLogin(string userName, int result, string message, Dictionary<string, List<string>> lunchDic)
        {
            // -1 => 실패 (없는 계정 정보)
            // 0 => 성공

            if (result == 0)
            {
                if (selectionChain != null)
                {
                    selectionChain.Dispose();
                    selectionChain = null;
                }
             
                TM_TCPClient.Instance.UserName = userName;

                lunchHandler.InstantiateCurrentLunches(lunchDic);

                onLoginedEvent.Invoke();
                toastMessage.HideForcelly();
            }
            else if (result == 1)
            {
                toastMessage.Show("로그인에 실패하였습니다\n계정 정보를 확인하세요.");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected virtual void OnResponseCreateAccount(int result, string message)
        {
            if (result == -1)
            {
                toastMessage.Show("이미 등록된 아이디 혹은 이름이 존재합니다.");
            }
            else if (result == 0)
            {
                toastMessage.Show("회원가입에 성공하셨습니다.\n가입한 계정으로 로그인하세요.");

                createAccountPanelIDInputField.text = "";
                createAccountPanelPasswordInputField.text = "";
                createAccountNameInputField.text = "";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}