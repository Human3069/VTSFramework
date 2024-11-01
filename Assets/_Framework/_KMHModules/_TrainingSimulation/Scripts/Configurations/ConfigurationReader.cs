using Cysharp.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class ConfigurationReader : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[ConfigurationReader]</b></color> {0}";
        private const string CONFIG_LOCAL_PATH = "/Configurations/ClientConfiguration.xml";

        protected static ConfigurationReader _instance;
        public static ConfigurationReader Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [ReadOnly]
        public ClientConfiguration ClientConfig;

        protected bool isXMLReadDone = false;

        public async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return isXMLReadDone == true;
            }
        }

        protected void Awake()
        {
            Debug.Assert(isXMLReadDone == false);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            PostAwake().Forget();
        }

        protected async UniTaskVoid PostAwake()
        {
            string fullPath = Application.streamingAssetsPath + CONFIG_LOCAL_PATH;
            if (File.Exists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "fullPath : " + CONFIG_LOCAL_PATH + " Not Exist File!");

                return;
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClientConfiguration));
            StreamReader _streamReader = new StreamReader(fullPath);

            await UniTask.WaitUntil(delegate { return VTSManager.Instance != null; });
            ClientConfig = (ClientConfiguration)xmlSerializer.Deserialize(_streamReader.BaseStream);
            ClientConfig = ClientConfig.Parsed();

            _streamReader.Close();
            isXMLReadDone = true;
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}