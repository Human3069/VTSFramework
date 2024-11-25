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
                    toastMessage.Show("���̵� �Է��ϼ���");
                }
                else if (string.IsNullOrEmpty(password) == true)
                {
                    toastMessage.Show("��й�ȣ�� �Է��ϼ���");
                }
                else
                {
                    toastMessage.Show("�ε���...", 60f, true);
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
                    toastMessage.Show("���̵� �Է��ϼ���");
                }
                else if (userID.ToLower().Contains("admin") == true)
                {
                    toastMessage.Show("admin �� ���Ե� ���̵�� �� �� �����ϴ�.");
                }
                else if (string.IsNullOrEmpty(userPassword) == true)
                {
                    toastMessage.Show("��й�ȣ�� �Է��ϼ���");
                }
                else if (string.IsNullOrEmpty(userName) == true)
                {
                    toastMessage.Show("�̸��� �Է��ϼ���");
                }
                else if (userName.ToLower().Contains("admin") == true)
                {
                    toastMessage.Show("admin �� ���Ե� �̸��� �� �� �����ϴ�.");
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
            // -1 => ���� (���� ���� ����)
            // 0 => ����

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
                toastMessage.Show("�α��ο� �����Ͽ����ϴ�\n���� ������ Ȯ���ϼ���.");
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
                toastMessage.Show("�̹� ��ϵ� ���̵� Ȥ�� �̸��� �����մϴ�.");
            }
            else if (result == 0)
            {
                toastMessage.Show("ȸ�����Կ� �����ϼ̽��ϴ�.\n������ �������� �α����ϼ���.");

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