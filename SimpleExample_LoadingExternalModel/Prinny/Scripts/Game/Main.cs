using UnityEngine;
using BepInEx;
using System.Collections;
using System.IO;

namespace Prinny
{
    [BepInPlugin("com.Prinny.DSP", "Prinny_240225", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        #region 这些代码是用来加载AB包的具体方法（如果协程调用失败，请复制到主线程入口所在类使用）

        //↓--------------------------------------------------------------------------------------------------------------------↓
        //↓--------------------------------------------Unity_ABTestFuncStart---------------------------------------------------↓
        //↓--------------------------------------------------------------------------------------------------------------------↓
        //注：当这些函数在Unity中使用正常，素材也实例化成功，但进行BepInEx制作MOD时报错：①读取文件报header问题；②读取字节则内存数据无法成功解压
        //上述故障原因跟Unity版本有关，建议用游戏同版本Unity去转AB包素材

        //ABTest_CustomGlobalValues，这些全局字段在下面函数里接收获取到的素材
        public GameObject[] gameObjectGroup;
        public AssetBundle assetBundle;
        public IEnumerator currentIEnumerator;
        public Coroutine currentCoroutine;
        public bool isCoroutineRunning = false;
        /// <summary>
        /// 保存所有活动协程的列表，需要Resource.multiCoroutine = true时可用
        /// </summary>
        //public List<Coroutine> activeCoroutines = new List<Coroutine>(); //取消多协程
        //public bool multiCoroutine = false;

        //public Resource()
        //{
        //    Debug.Log("Prinny: Resource 对象已建立！");
        //}

        //~Resource()
        //{
        //    Debug.Log("Prinny: Resource 对象已摧毁！");
        //}

        //ABTest_CustomFuncTemplates

        /// <summary>
        /// 同步加载: 以文件的形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromFile(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        public void ABLoadFromFile(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以byte[] 形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromMemory(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromMemory(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以流的形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromStream(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
        {
            assetBundle = AssetBundle.LoadFromStream(File.OpenRead(path));
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以流的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromStream(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromStream(File.OpenRead(path));
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 异步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromFileAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromMemoryAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载AB包中全资源（存储在Resource.gameObjectGroup、Resource.assetBundle）：
        /// 以byte[] 形式加载整个AssetBundle到内存中，而不是从流中加载它，这可能更高效，用于AssetBundle较小或需快速访问AssetBundle中的所有资产。
        /// 但AssetBundle很大则可能会导致内存使用增加，该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载。
        /// 在主角位置实例化示范：
        /// GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerator ABLoadAllFromMemoryAsync(string path)
        {
            Debug.Log("Prinny: 进入协程！");
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            Debug.Log("Prinny: 协程处理完成，素材已加载！");
            assetBundle = request.assetBundle;
            gameObjectGroup = assetBundle.LoadAllAssets<GameObject>();
            Debug.Log("Prinny: 素材已赋值给实例字段！");
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
            Debug.Log("Prinny: 协程已关闭！");
        }

        /// <summary>
        /// 异步加载: 以流的形式加载AssetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromStreamAsync(string path, string resName)
        {
            // AssetBundleCreateRequest 类是 AssetBundle 的一个实例，它表示一个异步加载请求。AssetBundle.LoadFromStreamAsync 方法使用指定路径的文件流来创建一个新的 AssetBundleCreateRequest 实例。
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(File.OpenRead(path));
            // yield return 语句用于将控制权交还调用上下文，直到异步加载请求完成。一旦请求完成，该方法将使用 LoadAsset 方法从 AssetBundle 中加载指定名称的游戏对象。
            // 加载游戏对象后，该方法将通过 yield return 语句返回对 obj 变量的引用，这使得调用代码可以使用该对象。
            // 总的来说，此方法可用于在游戏或应用程序中异步加载 AssetBundle，并检索其中的游戏对象。这可以在需要时帮助优化内存使用和加载时间。
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromFileAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromFileAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromMemoryAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromMemoryAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载AB包中全资源（存储在Resource.gameObjectGroup、Resource.assetBundle）：
        /// 以byte[] 形式加载整个AssetBundle到内存中，而不是从流中加载它，这可能更高效，用于AssetBundle较小或需快速访问AssetBundle中的所有资产。
        /// 但AssetBundle很大则可能会导致内存使用增加，该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载。
        /// 在主角位置实例化示范：
        /// GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void LoadAllFromMemoryAsync(string path)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                Debug.Log("Prinny: LoadAllFromMemoryAsync => " + path);
                currentIEnumerator = ABLoadAllFromMemoryAsync(path);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载: 以流的形式加载AssetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromStreamAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromStreamAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 停止协程（不再异步加载资源，无法对同步加载起效）。
        /// </summary>
        public void StopCoroutine()
        {
            if (currentCoroutine != null && isCoroutineRunning)
            {
                //有协程实例在运行则停止
                StopCoroutine(currentCoroutine);
                //变量重置
                currentIEnumerator = null;
                isCoroutineRunning = false;
            }
        }

        /// <summary>
        /// 步进协程（异步加载资源），让没完成的协程继续往下跑。
        /// MoveNext()方法一般不是直接调用的，在Unity中协程的推进是由Unity的引擎自动管理的。
        /// 当协程挂起（yield return）等待某个操作完成时，引擎会在适当的时候自动调用MoveNext()来恢复协程的执行，故不需要（也不应该）手动调用MoveNext()。
        /// 另如果同一特征的协程方法被执行多次（绑定多个协程实例）的话，无法精准操作每个实例个体的步进，会形成批量操作（慎用）。
        /// </summary>
        public void MoveNext()
        {
            //协程实例在运行（即使协程方法相同，每次诞生的协程实例并不一致）
            if (currentCoroutine != null && isCoroutineRunning)
            {
                //这里只能找代表协程方法的实例（同一协程方法的该实例仅有1个）去完成下一步
                currentIEnumerator.MoveNext(); //内部检索所有绑定的Coroutine并执行，如绑多个Coroutine的话无法精准操作每个个体只能批量操作（就算取消了本类的多协程方式也无法解决，慎用）
                // 再次检查协程是否完成
                if (currentCoroutine == null)
                {
                    //完成则变量重置
                    currentIEnumerator = null;
                    isCoroutineRunning = false;
                }
            }
        }

        //↑--------------------------------------------------------------------------------------------------------------------↑
        //↑--------------------------------------------Unity_ABTestFuncEnd-----------------------------------------------------↑
        //↑--------------------------------------------------------------------------------------------------------------------↑

        #endregion

        public GameObject odin;
        public GameObject icarus;
        ExploreSlice exploreSlice;

        //↓入口函数处运用示范↓
        private void Awake()
        {
            //开启一个协程进行资源加载
            LoadAllFromMemoryAsync("BepInEx/plugins/DSP_Battle_AssetBundles/abtest");
            //协程结束前尚无法马上取得素材，请等待

            //Unity编辑器中Application.dataPath返回Assets文件夹路径，打包后为应用程序所在路径
            //LoadAllFromMemoryAsync(Application.dataPath + "/AssetBundle/abtest");
        }

        private void Start()
        {
            if (!isCoroutineRunning)
            {
                Debug.Log("gameObjectGroup.Length => " + gameObjectGroup.Length.ToString());
                for (int i = 0; i < gameObjectGroup.Length; i++)
                {
                    Debug.Log("读取AB包中第" + i.ToString() + "个元素成功！");
                    Debug.Log("GameObject " + i + " Name: " + gameObjectGroup[i].name);
                }
            }
            exploreSlice = new ExploreSlice();
            Debug.Log("挂载ExploreSlice模型爆炸功能！");
        }

        private void Update()
        {
            #region 外部模型导入

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("按下了Q键，执行功能测试");

                //实例化资源中第一个游戏物体（前提是资源已经在协程里加载完毕，这里要进行检查）
                if (!isCoroutineRunning)
                {
                    icarus = GameObject.Find("Player (Icarus 1)");
                    //if (icarus != null)
                    //{
                    //    Debug.Log("伊卡洛斯当前位置");
                    //    Debug.Log("GameMain.mainPlayer.gameObject.name: " + GameMain.mainPlayer.gameObject.name);
                    //    Debug.Log("GameMain.mainPlayer.gameObject.transform.position: " + GameMain.mainPlayer.gameObject.transform.position.ToString());
                    //    Debug.Log("GameMain.mainPlayer.gameObject.transform.rotation: " + GameMain.mainPlayer.gameObject.transform.rotation.ToString());
                    //    Debug.Log("GameMain.mainPlayer.transform.position: " + GameMain.mainPlayer.transform.position.ToString());
                    //    Debug.Log("GameMain.mainPlayer.transform.rotation: " + GameMain.mainPlayer.transform.rotation.ToString());
                    //    Debug.Log("GameMain.mainPlayer.transform.localPosition: " + GameMain.mainPlayer.transform.localPosition.ToString());
                    //    Debug.Log("GameMain.mainPlayer.transform.localRotation: " + GameMain.mainPlayer.transform.localRotation.ToString());
                    //    Debug.Log("GameMain.mainPlayer.uPosition: " + GameMain.mainPlayer.uPosition.ToString());
                    //    Debug.Log("GameMain.mainPlayer.uRotation: " + GameMain.mainPlayer.uRotation.ToString());
                    //    //[Info: Unity Log] GameMain.mainPlayer.gameObject.name: Player(Icarus 1)
                    //    //[Info: Unity Log] GameMain.mainPlayer.gameObject.transform.position: (161.3, -12.3, 118.0)
                    //    //[Info: Unity Log] GameMain.mainPlayer.gameObject.transform.rotation: (0.6, 0.3, -0.3, 0.6)
                    //    //[Info: Unity Log] GameMain.mainPlayer.transform.position: (161.3, -12.3, 118.0)
                    //    //[Info: Unity Log] GameMain.mainPlayer.transform.rotation: (0.6, 0.3, -0.3, 0.6)
                    //    //[Info: Unity Log] GameMain.mainPlayer.transform.localPosition: (161.3, -12.3, 118.0)
                    //    //[Info: Unity Log] GameMain.mainPlayer.transform.localRotation: (0.6, 0.3, -0.3, 0.6)
                    //    //[Info: Unity Log] GameMain.mainPlayer.uPosition: [-34931.1093807605,-753.175833974017,-19735.9561231628]
                    //    //[Info: Unity Log] GameMain.mainPlayer.uRotation: (0.0, 0.7, 0.7, -0.1)
                    //}

                    if (icarus != null)
                    {
                        //gameObjectGroup[0]是奥丁，gameObjectGroup[1]是跳虫，目前AB包（abtest）内这只有2个预制体。
                        odin = Instantiate(gameObjectGroup[0]);
                        //odin = Instantiate(gameObjectGroup[0], icarus.transform.position, icarus.transform.rotation);
                        //odin = Instantiate(gameObjectGroup[0], GameMain.mainPlayer.transform.position, GameMain.mainPlayer.transform.rotation);
                        //GameObject odin = GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.transform.position, Quaternion.identity);

                        Debug.Log("奥丁已创建");
                        //Debug.Log("odin.transform.position: " + odin.transform.position.ToString());
                        //Debug.Log("odin.transform.rotation: " + odin.transform.rotation.ToString());
                        //Debug.Log("odin.transform.localPosition: " + odin.transform.localPosition.ToString());
                        //Debug.Log("odin.transform.localRotation: " + odin.transform.localRotation.ToString());

                    }

                    //检测动画剪辑
                    Animation[] animations = odin.GetComponents<Animation>();
                    foreach (Animation animation in animations)
                    {
                        Debug.Log("odinAnimation Name: " + animation.name);
                        Debug.Log("odinCurrentAnimationClip Name: " + animation.clip.name);
                    }

                    //删除预制体内的刚体，防止子物体参与物理引擎，让子模型完全按主体的Transform行动
                    //Rigidbody odinRigidbody = odin.GetComponent<Rigidbody>();
                    //if (odinRigidbody != null)
                    //{
                    //    Destroy(odinRigidbody);
                    //    Debug.Log("删除odin刚体组件，防止Transform被物理引擎修改");
                    //}

                    #region 对游戏物体进行镭射检测并输出碰撞到的物体名

                    //LayerMask layerMask = LayerMask.GetMask("指定层的LayerName");

                    //layerMask设置为 ~0，这将包含所有 32 个可用层级。
                    //LayerMask layerMask = ~0;

                    // 在 mainPlayer 游戏对象周围指定半径内检测游戏对象
                    Debug.Log("对周围10.0半径内的游戏对象进行镭射检测...");

                    #region 全层检测

                    LayerMask layerMask = ~0;
                    Collider[] hits = Physics.OverlapSphere(GameMain.mainPlayer.transform.position, 10.0f, layerMask);
                    // 输出检测到的游戏对象的名称
                    foreach (Collider hit in hits)
                    {
                        //得到游戏对象
                        GameObject hitObject = hit.gameObject;
                        Debug.Log("检测到游戏对象: " + hitObject.name);
                        //hit.gameObject.AddComponent<DrawBounds>();
                        //Debug.Log("挂载爆破功能: " + hitObject.name);
                    }

                    #endregion

                    #endregion

                    #region 衔接

                    //衔接前游戏物体的世界坐标系的旋转和位置与要衔接的主体保持一致
                    odin.transform.localPosition = GameMain.mainPlayer.transform.localPosition;
                    odin.transform.localRotation = GameMain.mainPlayer.controller.model.localRotation;
                    //odin.transform.position = GameMain.mainPlayer.uPosition;
                    //odin.transform.rotation = GameMain.mainPlayer.uRotation;

                    //将odin设置为mainPlayer的子对象（直接拼装了，比下方的每帧修正更省事）
                    odin.transform.parent = GameMain.mainPlayer.transform;
                    Debug.Log("奥丁已拼接到mainPlayer");
                    //odin.transform.SetParent(GameMain.mainPlayer.gameObject.transform);

                    //Debug.Log("拼接后奥丁的变换属性：");
                    //Debug.Log("odin.transform.position: " + odin.transform.position.ToString());
                    //Debug.Log("odin.transform.rotation: " + odin.transform.rotation.ToString());
                    //Debug.Log("odin.transform.localPosition: " + odin.transform.localPosition.ToString());
                    //Debug.Log("odin.transform.localRotation: " + odin.transform.localRotation.ToString());

                    #endregion

                    #region 给odin添加刚体组件

                    //获取mainPlayer上的刚体组件
                    //Rigidbody mainPlayerRigidbody = GameMain.mainPlayer.gameObject.GetComponent<Rigidbody>();

                    //获取odin上的刚体组件
                    Rigidbody odinRigidbody = odin.AddComponent<Rigidbody>();

                    ////复制刚体参数
                    //odinRigidbody.mass = mainPlayerRigidbody.mass;
                    //odinRigidbody.drag = mainPlayerRigidbody.drag;
                    //odinRigidbody.angularDrag = mainPlayerRigidbody.angularDrag;
                    //odinRigidbody.useGravity = mainPlayerRigidbody.useGravity;
                    //odinRigidbody.isKinematic = mainPlayerRigidbody.isKinematic;
                    //odinRigidbody.interpolation = mainPlayerRigidbody.interpolation;
                    //odinRigidbody.sleepThreshold = mainPlayerRigidbody.sleepThreshold;
                    //odinRigidbody.velocity = mainPlayerRigidbody.velocity;
                    //odinRigidbody.angularVelocity = mainPlayerRigidbody.angularVelocity;

                    #endregion

                    #region 给odin添加网格碰撞器组件

                    //MeshCollider odinMeshCollider = odin.AddComponent<MeshCollider>();
                    //if (odinMeshCollider.sharedMesh != null) { Debug.Log("odinMeshCollider 名字: " + odinMeshCollider.name); }
                    //MeshFilter meshFilter = gameObjectGroup[0].GetComponentInChildren<MeshFilter>();
                    //if (meshFilter != null)
                    //{
                    //    Debug.Log("Found mesh: " + meshFilter.sharedMesh.name + " in prefab: " + gameObjectGroup[0].name);
                    //    odinMeshCollider.sharedMesh = meshFilter.sharedMesh;
                    //}
                    //MeshRenderer meshRenderer = odin.AddComponent<MeshRenderer>();
                    //meshRenderer.enabled = true;

                    //foreach (var prefab in gameObjectGroup)
                    //{
                    //    MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();
                    //    if (meshFilter != null)
                    //    {
                    //        Debug.Log("Found mesh: " + meshFilter.sharedMesh.name + " in prefab: " + prefab.name);
                    //        odinMeshCollider.sharedMesh = meshFilter.sharedMesh;
                    //        break;
                    //    }
                    //}

                    //MeshFilter[] odinMeshFilters = odin.GetComponentsInChildren<MeshFilter>();
                    //foreach (MeshFilter odinMeshFilter in odinMeshFilters)
                    //{
                    //    Debug.Log("MeshFilter 名字: " + odinMeshFilter.name);
                    //}

                    //// 获取MM_Odin预制体内的Mesh对象
                    //MeshFilter odinMeshFilter = gameObjectGroup[0].GetComponentInChildren<MeshFilter>();
                    //if (odinMeshFilter != null)
                    //{
                    //    Mesh mmOdinMesh = odinMeshFilter.sharedMesh;

                    //    if (mmOdinMesh != null)
                    //    {
                    //        // 给Odin添加Mesh碰撞器，并指定MM_Odin的Mesh
                    //        MeshCollider odinMeshCollider = odin.AddComponent<MeshCollider>();
                    //        odinMeshCollider.convex = false; // 如果需要，可以将其设置为true以创建一个凸包碰撞器
                    //        odinMeshCollider.sharedMesh = mmOdinMesh;
                    //        Debug.Log("已将名为\"" + mmOdinMesh.name + "\"的Mesh添加到Odin的Mesh碰撞器中。");
                    //    }
                    //}

                    #endregion

                }
                else { Debug.Log("协程未完成！依然读取AB包中..."); }
            }
            #endregion

            #region 行走动画

            if (odin != null && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                odin.GetComponent<Animation>().Play("Walk");
            }

            #endregion

            #region 变换检测

            //if (odin != null && Input.GetKeyDown(KeyCode.N))
            //{
            //    Debug.Log("当前奥丁变换属性：");
            //    Debug.Log("odin.transform.position: " + odin.transform.position.ToString());
            //    Debug.Log("odin.transform.rotation: " + odin.transform.rotation.ToString());
            //    Debug.Log("odin.transform.localPosition: " + odin.transform.localPosition.ToString());
            //    Debug.Log("odin.transform.localRotation: " + odin.transform.localRotation.ToString());
            //    Debug.Log("当前伊卡洛斯变换属性：");
            //    Debug.Log("GameMain.mainPlayer.gameObject.name: " + GameMain.mainPlayer.gameObject.name);
            //    Debug.Log("GameMain.mainPlayer.gameObject.transform.position: " + GameMain.mainPlayer.gameObject.transform.position.ToString());
            //    Debug.Log("GameMain.mainPlayer.gameObject.transform.rotation: " + GameMain.mainPlayer.gameObject.transform.rotation.ToString());
            //    Debug.Log("GameMain.mainPlayer.transform.position: " + GameMain.mainPlayer.transform.position.ToString());
            //    Debug.Log("GameMain.mainPlayer.transform.rotation: " + GameMain.mainPlayer.transform.rotation.ToString());
            //    Debug.Log("GameMain.mainPlayer.transform.localPosition: " + GameMain.mainPlayer.transform.localPosition.ToString());
            //    Debug.Log("GameMain.mainPlayer.transform.localRotation: " + GameMain.mainPlayer.transform.localRotation.ToString());
            //    Debug.Log("GameMain.mainPlayer.uPosition: " + GameMain.mainPlayer.uPosition.ToString());
            //    Debug.Log("GameMain.mainPlayer.uRotation: " + GameMain.mainPlayer.uRotation.ToString());
            //}

            #endregion

            #region 测试手动旋转

            //// 创建一个新的四元数，表示沿x轴旋转90度
            //Quaternion xRotationQuaternion = Quaternion.AngleAxis(90f, Vector3.right);
            //// 创建一个新的四元数，表示沿y轴旋转90度
            //Quaternion yRotationQuaternion = Quaternion.AngleAxis(90f, Vector3.up);
            //// 创建一个新的四元数，表示沿z轴旋转90度
            //Quaternion zRotationQuaternion = Quaternion.AngleAxis(90f, Vector3.forward);

            //if (odin != null && Input.GetKeyDown(KeyCode.X))
            //{
            //    // 将旋转四元数应用到odin的局部旋转上
            //    odin.transform.localRotation = odin.transform.localRotation * xRotationQuaternion;
            //}
            //if (odin != null && Input.GetKeyDown(KeyCode.Y))
            //{
            //    // 将旋转四元数应用到odin的局部旋转上
            //    odin.transform.localRotation = odin.transform.localRotation * yRotationQuaternion;
            //}
            //if (odin != null && Input.GetKeyDown(KeyCode.Z))
            //{
            //    // 将旋转四元数应用到odin的局部旋转上
            //    odin.transform.localRotation = odin.transform.localRotation * zRotationQuaternion;
            //}

            #endregion

            #region 自动旋转

            if (odin != null)
            {
                // 将旋转四元数应用到odin的局部旋转上
                odin.transform.localRotation = GameMain.mainPlayer.controller.model.localRotation;

                //odin.transform.localRotation = Quaternion.Inverse(GameMain.mainPlayer.uRotation);
                //odin.transform.localRotation = GameMain.mainPlayer.uRotation * yRotationQuaternion;
                //odin.transform.localRotation = GameMain.mainPlayer.uRotation * zRotationQuaternion;

            }

            //if (odin != null)
            //{
            //    //对每次获取的uRotation进行修正后赋值给odin.transform.localRotation
            //    odin.transform.localRotation = GameMain.mainPlayer.uRotation * xRotationQuaternion;
            //}

            #endregion

            if (odin != null && Input.GetMouseButtonDown(0))
            {
                // 获取鼠标点击的屏幕坐标
                Vector3 mousePos = Input.mousePosition;
                Vector3 rayOrigin = Camera.main.transform.position; // 射线的起点：摄像机位置
                Vector3 rayDirection = Camera.main.ScreenPointToRay(mousePos).direction; // 射线的方向：从摄像机到鼠标点击

                // 投射射线
                Ray ray = new Ray(rayOrigin, rayDirection);
                RaycastHit hit;
                float rayLength = 10000f; // 射线的长度，可以根据需要调整
                //LayerMask layerMask = LayerMask.GetMask("Default"); // 射线投射的目标层
                LayerMask layerMask = ~0;

                if (Physics.Raycast(ray, out hit, rayLength, layerMask))
                {
                    // 射线击中了物体
                    Debug.Log("Hit object: " + hit.transform.name);
                    if (hit.transform.gameObject.GetComponent<DrawBounds>() == null)
                    {
                        hit.transform.gameObject.AddComponent<DrawBounds>();
                        Debug.Log("挂载爆破功能: " + hit.transform.name);
                    }


                    exploreSlice.Explore(hit.transform.gameObject, ray.direction);
                }
                else
                {
                    // 射线没有击中任何物体
                    Debug.Log("No object hit");
                    // 使用Debug.DrawRay在Scene视图中绘制射线
                    Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red, 0.5f); // 红色射线，持续0.5秒
                }

            }
        }
    }
}
