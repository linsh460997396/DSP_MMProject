using System.IO;
using UnityEngine;
using BepInEx;
using System.Collections;

namespace Prinny
{
    [BepInPlugin("com.Prinny.DSP", "Prinny_240205", "1.0.0")]
    public class Prinny_Main : BaseUnityPlugin
    {
        #region 这些代码是用来加载AB包的具体方法

        //↓--------------------------------------------------------------------------------------------------------------------↓
        //↓--------------------------------------------Unity_ABTestFuncStart---------------------------------------------------↓
        //↓--------------------------------------------------------------------------------------------------------------------↓
        //注：当这些函数在Unity中使用正常，素材也实例化成功，但进行BepInEx制作MOD时报错：①读取文件报header问题；②读取字节则内存数据无法成功解压
        //故障原因跟Unity版本有关，建议用游戏同版本Unity去转AB包素材

        //ABTest_CustomGlobalValues，这些全局变量在下面函数里接收获取到的素材
        GameObject[] testOB;
        AssetBundle testAB;

        //ABTest_CustomFuncTemplates
        //同步加载: 以文件的形式加载AssetBundle
        private void TestLoadFromFile(string path, string resName)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            GameObject obj = ab.LoadAsset<GameObject>(resName);
            // TODO...
        }

        //同步加载: 以byte[] 形式加载AssetBundle
        private void TestLoadFromMemory(string path, string resName)
        {
            AssetBundle ab = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            GameObject obj = ab.LoadAsset<GameObject>(resName);
            // TODO...
            testOB[0] = obj;
        }

        //同步加载: 以流的形式加载AssetBundle
        private void TestLoadFromStream(string path, string resName)
        {
            AssetBundle ab = AssetBundle.LoadFromStream(File.OpenRead(path));
            GameObject obj = ab.LoadAsset<GameObject>(resName);
            // TODO...
        }

        //异步加载: 以文件的形式加载AssetBundle
        private IEnumerator TestLoadFromFileAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            GameObject obj = request.assetBundle.LoadAsset<GameObject>(resName);
            // TOOD...
            testOB[0] = obj;
            StopCoroutine(TestLoadFromFileAsync(path, resName));
        }


        //异步加载: 以byte[] 形式加载AssetBundle
        private IEnumerator TestLoadFromMemoryAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            GameObject obj = request.assetBundle.LoadAsset<GameObject>(resName);
            // TODO...
            testOB[0] = obj;
            StopCoroutine(TestLoadFromMemoryAsync(path, resName));
        }

        //异步加载AB包中全资源: 以byte[] 形式加载AssetBundle,然后实例化第一个
        private IEnumerator TestLoadFromMemoryAsyncNew(string path)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            AssetBundle lv_ab = request.assetBundle;
            // TODO...
            testOB = lv_ab.LoadAllAssets<GameObject>();
            //GameObject.Instantiate(testOB[0]);
            //GameObject.Instantiate(testOB[1]);
            GameObject.Instantiate(testOB[0], GameMain.mainPlayer.position, Quaternion.identity);
            GameObject.Instantiate(testOB[1], GameMain.mainPlayer.position, Quaternion.identity);
            //GameObject.Instantiate(testOB[1], new Vector3(10, 120, 10), Quaternion.identity);
            //GameObject.Instantiate(testOB[2], Camera.ScreenToWorldPoint(), Quaternion.identity);
            if (testOB[0] != null)
            {
                Debug.Log("读取AB包中第一个元素成功！");
            }
            if (testOB[1] != null)
            {
                Debug.Log("读取AB包中第二个元素成功！");
            }
            StopCoroutine(TestLoadFromMemoryAsyncNew(path));
        }


        //异步加载: 以流的形式加载AssetBundle
        private IEnumerator TestLoadFromStreamAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(File.OpenRead(path));
            yield return request;
            GameObject obj = request.assetBundle.LoadAsset<GameObject>(resName);
            // TODO...
        }

        //Start()中调用同步加载, 以TestLoadFromMemory()为例
        private void ABStart_01()
        {
            string path = "BepInEx/plugins/DSP_Battle_AssetBundles/abtest";
            string resName = "abtest";

            TestLoadFromMemory(path, resName);
        }

        //Start()中调用异步加载, 以TestLoadFromFileAsync()为例
        private void ABStart_02()
        {
            string path = "BepInEx/plugins/DSP_Battle_AssetBundles/abtest";
            string resName = "abtest";
            StartCoroutine(TestLoadFromFileAsync(path, resName));

            //异步加载这里采用的处理方式是Unity的协程,用于执行协程方法↓
            //StartCoroutine(TestLoadFromFileAsync(path, resName)); //去入口函数执行协程，开始资源加载和赋值
            //StopCoroutine(TestLoadFromFileAsync(path, resName)); //结束协程也可写在TestLoadFromFileAsync内的尾行，让任务结束后自行中止
        }

        //Start()中调用异步加载, 以TestLoadFromMemoryAsync()为例
        private void ABStart_03()
        {
            string path = "BepInEx/plugins/DSP_Battle_AssetBundles/abtest";
            string resName = "abtest";
            StartCoroutine(TestLoadFromMemoryAsync(path, resName));

            //异步加载这里采用的处理方式是Unity的协程,用于执行协程方法↓
            //StartCoroutine(TestLoadFromMemoryAsync(path, resName)); //去入口函数执行协程，开始资源加载和赋值
            //StopCoroutine(TestLoadFromMemoryAsync(path, resName)); //结束协程也可写在TestLoadFromMemoryAsync内的尾行，让任务结束后自行中止
        }

        //Start()中调用异步加载全资源, 以TestLoadFromMemoryAsyncNew()为例，然后实例化第一个
        private void ABStart_04()
        {
            string path = "BepInEx/plugins/DSP_Battle_AssetBundles/abtest";
            StartCoroutine(TestLoadFromMemoryAsyncNew(path));
            //异步加载这里采用的处理方式是Unity的协程,用于执行协程方法↓
            //StartCoroutine(TestLoadFromMemoryAsyncNew(path)); //去入口函数执行协程，开始资源加载和赋值
            //StopCoroutine(TestLoadFromMemoryAsyncNew(path)); //结束协程也可写在TestLoadFromMemoryAsyncNew内的尾行，让任务结束后自行中止
        }
        //↑--------------------------------------------------------------------------------------------------------------------↑
        //↑--------------------------------------------Unity_ABTestFuncEnd-----------------------------------------------------↑
        //↑--------------------------------------------------------------------------------------------------------------------↑
        #endregion

        private void Start()
        {

        }

        private void Awake()
        {
            Debug.Log("Prinny: MOD加载完毕！");
        }


        private void Update()
        {
            #region 这些代码是测试机甲出现后，在机甲脚底位置创建外置模型实例的
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("按下了Q，只能创建一次哦");
                ABStart_04();

                #region 这些代码是用来测试加载AB包的各种方法
                //ABTest_CustomFuncActions
                //ABStart_01(); //同步加载, 以TestLoadFromMemory()为例
                //ABStart_02(); //异步加载, 以TestLoadFromFileAsync()为例
                //ABStart_03(); //异步加载, 以TestLoadFromMemoryAsync()为例
                //ABStart_04(); //异步加载，以TestLoadFromMemoryAsyncNew()为例，然后实例化第一个元素
                //ABStart_BepInExStyle(); //英灵神殿追加怪物MOD中使用BepInEx插件函数加载AB包案例
                #endregion

            }
            #endregion
        }

    }
}
