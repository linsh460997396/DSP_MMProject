using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using crecheng.DSPModSave;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using xiaoye97;

namespace DSP_Battle
{
    [BepInPlugin("com.ckcz123.DSP_Battle", "DSP_Battle", "2.1.2")]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency("Gnimaerd.DSP.plugin.MoreMegaStructure")]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry))]
    [CommonAPISubmoduleDependency(nameof(TabSystem))]
    [CommonAPISubmoduleDependency(nameof(LocalizationModule))]
    
    public class DspBattlePlugin : BaseUnityPlugin, IModCanSave
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

        public static string GUID = "com.ckcz123.DSP_Battle";
        public static string MODID_tab = "DSPBattle";

        public static System.Random randSeed = new System.Random();
        public static int pagenum;
        public static ManualLogSource logger;
        private static ConfigFile config;
        public static ConfigEntry<int> starCannonRenderLevel;
        public static ConfigEntry<bool> starCannonDirectionReverse;

        public static bool isControlDown = false;
        public void Awake()
        {
            logger = Logger;
            config = Config;
            Configs.Init(Config);

            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var resources = new ResourceData(GUID, "DSPBattle", pluginfolder);
            resources.LoadAssetBundle("dspbattletex");
            ProtoRegistry.AddResource(resources);
            try
            {
                using (ProtoRegistry.StartModLoad(GUID))
                {
                    //pagenum = TabSystem.RegisterTab($"{MODID_tab}:{MODID_tab}Tab", new TabData("轨道防御", "Assets/DSPBattle/dspbattletabicon"));
                    pagenum = MoreMegaStructure.MoreMegaStructure.pagenum;
                    BattleProtos.pageBias = (pagenum - 2) * 1000;
                    MoreMegaStructure.MoreMegaStructure.battlePagenum = pagenum;
                }
            }
            catch (Exception)
            {
                pagenum = 0;
            }
            starCannonRenderLevel = Config.Bind<int>("config", "StarCannonRenderLevel", 2, "[0-3] Higher Level will provide more star cannon effect and particles but might decrease the UPS and FPS when star cannon is firing. 更高的设置会提供更多的恒星炮特效，但可能会在恒星炮开火时降低帧率，反之则可能提高开火时的帧率。");
            starCannonDirectionReverse = Config.Bind<bool>("config", "starCannonDirectionReverse", false, "Deprecated. 已弃用。");

            
            MoreMegaStructure.StarCannon.renderLevel = starCannonRenderLevel.Value;
            MoreMegaStructure.StarCannon.renderLevel = MoreMegaStructure.StarCannon.renderLevel > 3 ? 3 : MoreMegaStructure.StarCannon.renderLevel;
            MoreMegaStructure.StarCannon.renderLevel = MoreMegaStructure.StarCannon.renderLevel < 0 ? 0 : MoreMegaStructure.StarCannon.renderLevel;
            //EnemyShips.Init();
            Harmony.CreateAndPatchAll(typeof(DspBattlePlugin));
            
            Harmony.CreateAndPatchAll(typeof(BattleProtos));
            Harmony.CreateAndPatchAll(typeof(FastStartOption));
            Harmony.CreateAndPatchAll(typeof(UIDialogPatch));
            Harmony.CreateAndPatchAll(typeof(Droplets));
            Harmony.CreateAndPatchAll(typeof(RendererSphere));
            Harmony.CreateAndPatchAll(typeof(PlanetEngine));
            Harmony.CreateAndPatchAll(typeof(UIRank));
            Harmony.CreateAndPatchAll(typeof(Rank));
            Harmony.CreateAndPatchAll(typeof(BattleBGMController));
            Harmony.CreateAndPatchAll(typeof(Relic));
            Harmony.CreateAndPatchAll(typeof(RelicFunctionPatcher));
            Harmony.CreateAndPatchAll(typeof(StarFortress));
            Harmony.CreateAndPatchAll(typeof(UIStarFortress));
            Harmony.CreateAndPatchAll(typeof(StationOrderFixPatch));
            Harmony.CreateAndPatchAll(typeof(DropletFleetPatchers));
            Harmony.CreateAndPatchAll(typeof(EventSystem));

            LDBTool.PreAddDataAction += BattleProtos.AddProtos;
            BattleProtos.AddTranslate();
            //LDBTool.PostAddDataAction += BattleProtos.PostDataAction;
            BattleProtos.InitEventProtos();
        }

        public void Start()
        {
            //BattleBGMController.InitAudioSources();

            //开启一个协程进行资源加载
            LoadAllFromMemoryAsync("BepInEx/plugins/DSP_Battle_AssetBundles/abtest");
            //协程结束前尚无法马上取得素材，请等待

            //Unity编辑器中Application.dataPath返回Assets文件夹路径，打包后为应用程序所在路径
            //LoadAllFromMemoryAsync(Application.dataPath + "/AssetBundle/abtest");
        }

        public void Update()
        {
            #region 外部模型导入测试

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("按下了Q键，执行功能测试");

                //实例化资源中第一个游戏物体（前提是资源已经在协程里加载完毕，这里要进行检查）
                if (GameMain.mainPlayer != null && !isCoroutineRunning)
                {
                    Vector3 targetPosition = GameMain.mainPlayer.transform.position;
                    Debug.Log("伊卡洛斯位置: " + targetPosition.ToString());

                    Debug.Log("gameObjectGroup.Length => " + gameObjectGroup.Length.ToString());
                    for (int i = 0; i < gameObjectGroup.Length; i++)
                    {
                        Debug.Log("读取AB包中第" + i.ToString() + "个元素成功！");
                        Debug.Log("GameObject " + i + " Name: " + gameObjectGroup[i].name);
                    }
                    //gameObjectGroup[0]是奥丁，gameObjectGroup[1]是跳虫，目前AB包（abtest）内这只有2个预制体。
                    GameObject odinMech = GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.transform.position, Quaternion.identity);
                    // 将odinMech设置为mainPlayer的子对象（直接拼装了，比下方的每帧修正更省事）

                    //mainPlayer的游戏物体名（实例对象ID字符串）
                    Debug.Log("mainPlayer对应的游戏物体ID：" + GameMain.mainPlayer.gameObject.name);

                    //LayerMask layerMask = LayerMask.GetMask("指定层的LayerName");

                    //layerMask设置为 ~0。这将包含所有 32 个可用层级。
                    LayerMask layerMask = ~0;

                    // 在 mainPlayer 游戏对象周围指定半径内检测游戏对象
                    Debug.Log("对周围10.0半径内的游戏对象镭射检测...");
                    Collider[] hits = Physics.OverlapSphere(GameMain.mainPlayer.transform.position, 10f, layerMask);

                    // 输出检测到的游戏对象的名称
                    foreach (Collider hit in hits)
                    {
                        //得到游戏对象
                        GameObject hitObject = hit.gameObject;
                        Debug.Log("检测到游戏对象: " + hitObject.name);
                        //odinMech.transform.parent = GameObject.Find(hitObject.name).transform;
                    }

                    //拼接模型到角色
                    odinMech.transform.parent = GameMain.mainPlayer.gameObject.transform;

                    //odinMech.transform.parent = GameObject.Find("Player(Icarus 1)").transform;

                    //odinMech.transform.localPosition = mainPlayer.transform.position;
                    //odinMech.transform.localRotation = mainPlayer.transform.rotation;
                    //odinMech.transform.localScale = mainPlayer.transform.localScale;
                }
                else { Debug.Log("协程未完成！依然读取AB包中..."); }
            }

            #endregion

            //if (Input.GetKeyDown(KeyCode.Minus) && !GameMain.isPaused && UIRoot.instance?.uiGame?.buildMenu?.currentCategory == 0 && (Configs.nextWaveState == 1 || Configs.nextWaveState == 2))
            //{
            //    Configs.nextWaveFrameIndex -= 60 * 60;
            //}
            if (Configs.developerMode && Input.GetKeyDown(KeyCode.Z))
            {
                //Debug.LogWarning("Z test warning by TCFV");
                //Debug.Log("Z test log by TCFV");
                //Debug.LogError("Z error log by TCFV");
                //EnemyShips.TestDestoryStation();
                Rank.AddExp(100000);
                if (MoreMegaStructure.MoreMegaStructure.curStar != null)
                {
                    int starIndex = MoreMegaStructure.MoreMegaStructure.curStar.index;
                    if (isControlDown)
                    {
                        //StarFortress.ConstructStarFortPoint(starIndex, 8037, 10000);
                        //StarFortress.ConstructStarFortPoint(starIndex, 8038, 10000);
                        //StarFortress.ConstructStarFortPoint(starIndex, 8039, 10000);
                    }
                    else
                    {
                        //StarFortress.ConstructStarFortPoint(starIndex, 8037, 743);
                        //StarFortress.ConstructStarFortPoint(starIndex, 8038, 743);
                        //StarFortress.ConstructStarFortPoint(starIndex, 8039, 743);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                isControlDown = true;
                UIStarFortress.RefreshSetBtnText();
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                isControlDown = false;
                UIStarFortress.RefreshSetBtnText();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && UIDevConsole.consoleObj != null && UIDevConsole.consoleObj.activeSelf)
            {
                DevConsole.PrevCommand();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && UIDevConsole.consoleObj != null && UIDevConsole.consoleObj.activeSelf)
            {
                DevConsole.NextCommand();
            }
            if (Configs.developerMode && isControlDown && Input.GetKeyDown(KeyCode.Z))
            {
                Relic.PrepareNewRelic();
                int planetId = 103;
                if (GameMain.localPlanet != null)
                    planetId = GameMain.localPlanet.id;
            }
            if (Configs.developerMode && isControlDown && Input.GetKeyDown(KeyCode.G))
            {
                EventSystem.InitNewEvent();
            }
            if (Configs.developerMode && isControlDown && Input.GetKeyDown(KeyCode.H))
            {
                EventSystem.ClearEvent();
            }
            if (Configs.developerMode && isControlDown && Input.GetKeyDown(KeyCode.J))
            {
                EventSystem.TestIfGroudBaseInited();
            }
            UIRelic.SelectionWindowAnimationUpdate();
            UIRelic.CheckRelicSlotsWindowShowByMouse();
            UIRelic.SlotWindowAnimationUpdate();
            UIEventSystem.OnUpdate();
            //BattleBGMController.BGMLogicUpdate();
            DevConsole.Update();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameMain), "OnDestroy")]
        public static void GameMain_onDestroy()
        {
            if (config == null) return;
            try
            {
                string configFile = config.ConfigFilePath;
                string path = Path.Combine(Path.GetDirectoryName(configFile), "LDBTool");
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception)
            { }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIMainMenu), "_OnOpen")]
        public static void UIMainMenu_OnOpen()
        {
            UpdateLogo();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIEscMenu), "_OnOpen")]
        public static void UIEscMenu_OnOpen()
        {
            UpdateLogo();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOption), "Apply")]
        public static void UpdateGameOption_Apply()
        {
            UpdateLogo();
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMain), "LateUpdate")]
        public static bool EscLogicBlocker()
        {
            UIDevConsole.EscLogic();
            UIEventSystem.EscLogic();
            return true;
        }

        public static void UpdateLogo()
        {
            var mainLogo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            var escLogo = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/logo");

            var iconstr = DSPGame.globalOption.languageLCID == 2052
                ? "Assets/DSPBattle/logocn"
                : "Assets/DSPBattle/logoen";
            var texture = Resources.Load<Sprite>(iconstr).texture;

            mainLogo.GetComponent<RawImage>().texture = texture;
            escLogo.GetComponent<RawImage>().texture = texture;
            mainLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            escLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAbnormalityTip), "_OnInit")]
        public static void UIAbnormalityTip_OnInit(ref UIAbnormalityTip __instance)
        {
            __instance.isWarned = true;
            __instance.willClose = true;
            __instance.mainTweener.Play1To0Continuing();
            __instance.closeDelayTime = 3f;
        }

        public static void InitStaticDataWhenLoad()
        { 
            BattleProtos.RewriteTutorialProtosWhenLoad();
            BattleProtos.EditProtossWhenLoad();
        }

        public void Export(BinaryWriter w)
        {
            Configs.Export(w);
            Droplets.Export(w);
            Rank.Export(w);
            Relic.Export(w);
            EventSystem.Exprot(w);
            //StarFortress.Export(w);
            //DevConsole.Export(w);
        }

        public void Import(BinaryReader r)
        {
            Configs.Import(r);
            Droplets.Import(r);
            Rank.Import(r);
            Relic.Import(r);
            EventSystem.Import(r);
            //StarFortress.Import(r);
            //DevConsole.Import(r);

            BattleProtos.ReCheckTechUnlockRecipes();
            BattleProtos.UnlockTutorials();
            //BattleBGMController.InitWhenLoad();

            InitStaticDataWhenLoad();
        }

        public void IntoOtherSave()
        {
            Configs.IntoOtherSave();
            //EnemyShips.IntoOtherSave();
            //MissileSilo.IntoOtherSave();
            Droplets.IntoOtherSave();
            Rank.IntoOtherSave();
            Relic.IntoOtherSave();
            EventSystem.IntoOtherSave();
            StarFortress.IntoOtherSave();

            DevConsole.IntoOtherSave();

            //EnemyShipUIRenderer.Init();
            //EnemyShipRenderer.Init();
            BattleProtos.ReCheckTechUnlockRecipes();
            BattleProtos.UnlockTutorials();
            //BattleBGMController.InitWhenLoad();

            InitStaticDataWhenLoad();
        }


    }
}

