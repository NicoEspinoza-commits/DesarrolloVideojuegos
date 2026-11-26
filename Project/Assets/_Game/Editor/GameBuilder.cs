// HERRAMIENTA DE EDITOR (no es parte del juego).
// Genera automáticamente sprites, sonidos, tiles, prefabs y escenas.
// Se ejecuta desde el menú: Herramientas > Construir juego.
// Una vez generado todo, puedes editar las escenas a mano normalmente.
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class GameBuilder
{
    const string Root = "Assets/_Game";
    const int GroundLayer = 6;

    // Mapa del nivel (cada carácter = 1 tile). Fila de arriba = y 5.
    //  # suelo   B ladrillo   c moneda   E enemigo   ^ pinchos
    //  P jugador F meta       ~ nube     * arbusto
    static readonly string[] LevelMap =
    {
        "     ~~~                ~~~~                      ~~~                 ~~~~",
        "",
        "",
        "                                 cc",
        "                                BBBB",
        "",
        "                          cc                                     cc",
        "                         BBBB           BBBB                    BBBB",
        "         ccc",
        "  P   *        E              ^       E             *   ^             E      F",
        "####################   ######################   #############  #################",
        "####################   ######################   #############  #################",
    };

    static Sprite sPlayer, sEnemy, sCoin, sGrass, sDirt, sBrick, sSpike, sFlag, sCloud, sBush;
    static AudioClip aJump, aCoin, aStomp, aClick, aMusic;
    static TileBase tGrass, tDirt, tBrick, tCloud, tBush;
    static GameObject pPlayer, pCoin, pEnemy, pSpike, pGoal;
    static Font font;

    [MenuItem("Herramientas/Construir juego")]
    public static void BuildAll()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (var f in new[] { "Scripts", "Sprites", "Audio", "Tiles", "Physics", "Prefabs", "Scenes" })
            Directory.CreateDirectory($"{Root}/{f}");
        if (AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.DeleteAsset("Assets/Scenes");

        SetupLayer();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateSprites();
        CreateAudio();
        CreateTiles();
        CreatePrefabs();
        CreateMainMenu();
        CreateLevel1();
        CreateGameOver();
        SetupProjectSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Juego construido.");
    }

    [MenuItem("Herramientas/Build Windows")]
    public static void BuildWindows()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { $"{Root}/Scenes/MainMenu.unity", $"{Root}/Scenes/Level1.unity", $"{Root}/Scenes/GameOver.unity" },
            locationPathName = "../Build/BlueyPixel.exe",
            target = BuildTarget.StandaloneWindows64,
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build: " + report.summary.result);
        if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // ---------- Configuración ----------
    static void SetupLayer()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tm.FindProperty("layers").GetArrayElementAtIndex(GroundLayer).stringValue = "Ground";
        tm.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetupProjectSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{Root}/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene($"{Root}/Scenes/Level1.unity", true),
            new EditorBuildSettingsScene($"{Root}/Scenes/GameOver.unity", true),
        };
        PlayerSettings.productName = "BlueyPixel";
        PlayerSettings.companyName = "NE Games";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        QualitySettings.vSyncCount = 1;
    }

    // ---------- Sprites (pixel art 16x16 dibujado por código) ----------
    static void CreateSprites()
    {
        Color clear = Color.clear, black = Color.black, white = Color.white;
        Color blue = new Color(0.2f, 0.45f, 0.95f), purple = new Color(0.55f, 0.2f, 0.7f);
        Color brown = new Color(0.55f, 0.33f, 0.18f), darkBrown = new Color(0.42f, 0.24f, 0.12f);
        Color green = new Color(0.25f, 0.75f, 0.3f), yellow = new Color(1f, 0.82f, 0.1f);
        Color brick = new Color(0.75f, 0.35f, 0.2f), gray = new Color(0.7f, 0.7f, 0.75f);

        sPlayer = MakeSprite("Player", 16, 16, (x, y) =>
        {
            if (y >= 9 && y <= 11 && (x == 6 || x == 10)) return black;               // pupilas
            if (y >= 9 && y <= 12 && (x == 5 || x == 6 || x == 9 || x == 10)) return white; // ojos
            if (y == 13 && x >= 3 && x <= 12) return new Color(1f, 0.85f, 0.2f);       // banda amarilla
            if (x >= 3 && x <= 12 && y <= 14 && y >= 2) return blue;                   // cuerpo
            if (y < 2 && (x == 4 || x == 5 || x == 10 || x == 11)) return darkBrown;   // pies
            return clear;
        });
        sEnemy = MakeSprite("Enemy", 16, 16, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 4f));
            if (y == 9 && (x == 5 || x == 10)) return black;                           // pupilas
            if (y >= 8 && y <= 9 && (x == 4 || x == 5 || x == 10 || x == 11)) return white;
            if (y == 11 && (x == 4 || x == 5 || x == 10 || x == 11)) return black;     // cejas
            return d < 8f && y >= 1 ? purple : (y == 0 && (x < 5 || x > 10) ? black : clear);
        });
        sCoin = MakeSprite("Coin", 16, 16, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
            if (d < 5f) return (x == 7 || x == 8) && y > 4 && y < 11 ? new Color(1f, 0.95f, 0.6f) : yellow;
            return d < 6.5f ? new Color(0.85f, 0.6f, 0f) : clear;
        });
        sGrass = MakeSprite("Grass", 16, 16, (x, y) => y >= 12 || (y == 11 && x % 3 == 0) ? green : ((x * 7 + y * 3) % 11 == 0 ? darkBrown : brown));
        sDirt = MakeSprite("Dirt", 16, 16, (x, y) => (x * 7 + y * 3) % 11 == 0 ? darkBrown : brown);
        sBrick = MakeSprite("Brick", 16, 16, (x, y) =>
        {
            bool mortar = y % 8 == 0 || (y < 8 ? x % 16 == 0 : x % 16 == 8);
            return mortar ? darkBrown : brick;
        });
        sSpike = MakeSprite("Spike", 16, 16, (x, y) =>
        {
            int local = x % 8;
            return y <= 8 && Mathf.Abs(local - 3.5f) <= (8 - y) * 0.5f ? gray : clear;
        });
        sFlag = MakeSprite("Flag", 16, 48, (x, y) =>
        {
            if (x >= 7 && x <= 8) return y == 47 || y == 46 ? yellow : white;          // mástil
            if (x > 8 && y >= 34 && y <= 45 && (x - 9) <= (6 - Mathf.Abs(y - 39.5f))) return green;
            return clear;
        });
        sCloud = MakeSprite("Cloud", 16, 16, (x, y) =>
        {
            bool blob = Vector2.Distance(new Vector2(x, y), new Vector2(8, 5)) < 6f || Vector2.Distance(new Vector2(x, y), new Vector2(4, 4)) < 4f;
            return blob && y >= 1 ? white : clear;
        });
        sBush = MakeSprite("Bush", 16, 16, (x, y) =>
            Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 0f)) < 8f ? new Color(0.15f, 0.6f, 0.25f) : clear);
    }

    static Sprite MakeSprite(string name, int w, int h, Func<int, int, Color> pixel)
    {
        var tex = new Texture2D(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, pixel(x, y));
        string path = $"{Root}/Sprites/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------- Audio (sonidos retro generados por código) ----------
    static void CreateAudio()
    {
        aJump = MakeWav("Jump", 0.15f, t => Square(Mathf.Lerp(300, 700, t / 0.15f), t) * (1 - t / 0.15f));
        aCoin = MakeWav("Coin", 0.25f, t => Square(t < 0.07f ? 988 : 1319, t) * (1 - t / 0.25f));
        aStomp = MakeWav("Stomp", 0.12f, t => Square(Mathf.Lerp(220, 60, t / 0.12f), t) * (1 - t / 0.12f));
        aClick = MakeWav("Click", 0.05f, t => Square(800, t) * (1 - t / 0.05f));

        // Música: melodía simple en bucle
        float[] notes = { 523, 659, 784, 659, 587, 698, 880, 698, 523, 659, 784, 1047, 988, 784, 659, 587 };
        float[] bass = { 131, 131, 175, 175, 131, 131, 196, 196 };
        const float noteLen = 0.25f;
        aMusic = MakeWav("Music", notes.Length * noteLen * 2, t =>
        {
            int i = (int)(t / noteLen);
            float inNote = t % noteLen;
            float melody = Square(notes[i % notes.Length], t) * Mathf.Clamp01(1 - inNote / noteLen) * 0.5f;
            float low = Mathf.Sin(2 * Mathf.PI * bass[(i / 2) % bass.Length] * t) * 0.5f;
            return (melody + low) * 0.6f;
        });
        foreach (var c in new[] { aMusic })
        {
            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(c)) as AudioImporter;
            var s = imp.defaultSampleSettings; s.loadType = AudioClipLoadType.CompressedInMemory; imp.defaultSampleSettings = s;
            imp.SaveAndReimport();
        }
    }

    static float Square(float freq, float t) => Mathf.Sin(2 * Mathf.PI * freq * t) >= 0 ? 0.3f : -0.3f;

    static AudioClip MakeWav(string name, float seconds, Func<float, float> sample)
    {
        const int rate = 22050;
        int count = (int)(seconds * rate);
        string path = $"{Root}/Audio/{name}.wav";
        using (var bw = new BinaryWriter(File.Create(path)))
        {
            bw.Write("RIFF".ToCharArray()); bw.Write(36 + count * 2); bw.Write("WAVE".ToCharArray());
            bw.Write("fmt ".ToCharArray()); bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(rate); bw.Write(rate * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write("data".ToCharArray()); bw.Write(count * 2);
            for (int i = 0; i < count; i++)
                bw.Write((short)(Mathf.Clamp(sample(i / (float)rate), -1f, 1f) * short.MaxValue));
        }
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    // ---------- Tiles ----------
    static void CreateTiles()
    {
        tGrass = MakeTile("Grass", sGrass, Tile.ColliderType.Grid);
        tDirt = MakeTile("Dirt", sDirt, Tile.ColliderType.Grid);
        tBrick = MakeTile("Brick", sBrick, Tile.ColliderType.Grid);
        tCloud = MakeTile("Cloud", sCloud, Tile.ColliderType.None);
        tBush = MakeTile("Bush", sBush, Tile.ColliderType.None);
    }

    static TileBase MakeTile(string name, Sprite sprite, Tile.ColliderType collider)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = collider;
        string path = $"{Root}/Tiles/{name}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(tile, path);
        return tile;
    }

    // ---------- Prefabs ----------
    static void CreatePrefabs()
    {
        var noFriction = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
        CreateAssetFresh(noFriction, $"{Root}/Physics/PlayerNoFriction.physicsMaterial2D");

        // Jugador
        var player = NewSprite("Player", sPlayer, 10);
        player.tag = "Player";
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var box = player.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.75f, 0.95f);
        box.sharedMaterial = noFriction;
        player.AddComponent<AudioSource>().playOnAwake = false;
        var feet = new GameObject("GroundCheck").transform;
        feet.SetParent(player.transform);
        feet.localPosition = new Vector3(0, -0.5f, 0);
        var pc = player.AddComponent<PlayerController>();
        Set(pc, "groundCheck", feet);
        Set(pc, "groundLayer", 1 << GroundLayer);
        Set(pc, "jumpSound", aJump);
        pPlayer = SavePrefab(player);

        // Moneda
        var coin = NewSprite("Coin", sCoin, 5);
        var cc = coin.AddComponent<CircleCollider2D>();
        cc.isTrigger = true; cc.radius = 0.4f;
        Set(coin.AddComponent<Coin>(), "pickupSound", aCoin);
        pCoin = SavePrefab(coin);

        // Enemigo
        var enemy = NewSprite("Enemy", sEnemy, 5);
        enemy.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        enemy.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.8f);
        Set(enemy.AddComponent<Enemy>(), "stompSound", aStomp);
        pEnemy = SavePrefab(enemy);

        // Pinchos
        var spike = NewSprite("Spike", sSpike, 5);
        var sb = spike.AddComponent<BoxCollider2D>();
        sb.isTrigger = true; sb.size = new Vector2(0.8f, 0.4f); sb.offset = new Vector2(0, -0.25f);
        spike.AddComponent<Hazard>();
        pSpike = SavePrefab(spike);

        // Meta
        var goal = NewSprite("Goal", sFlag, 4);
        var gb = goal.AddComponent<BoxCollider2D>();
        gb.isTrigger = true; gb.size = new Vector2(0.5f, 12f); gb.offset = new Vector2(0, 4.5f);
        goal.AddComponent<Goal>();
        pGoal = SavePrefab(goal);
    }

    static GameObject NewSprite(string name, Sprite sprite, int order)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return go;
    }

    static GameObject SavePrefab(GameObject go)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Root}/Prefabs/{go.name}.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void CreateAssetFresh(Object asset, string path)
    {
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    // Asigna un [SerializeField] privado desde el editor
    static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field) ?? throw new Exception($"Campo '{field}' no existe en {target}");
        switch (value)
        {
            case float f: p.floatValue = f; break;
            case int i: p.intValue = i; break;
            case string s: p.stringValue = s; break;
            case Object o: p.objectReferenceValue = o; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- Escenas ----------
    static Camera NewCameraAndLight(Color background)
    {
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = background;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();

        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        return cam;
    }

    static void NewMusic()
    {
        var music = new GameObject("Music").AddComponent<AudioSource>();
        music.clip = aMusic;
        music.loop = true;
        music.volume = 0.35f;
        music.playOnAwake = true;
    }

    static Transform NewCanvas()
    {
        var go = new GameObject("Canvas");
        go.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
        return go.transform;
    }

    static Text NewText(Transform parent, string content, int size, Vector2 anchor, Vector2 pos, Vector2 box)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var text = go.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        go.AddComponent<Outline>().effectColor = Color.black;
        return text;
    }

    static Button NewButton(Transform parent, string label, Vector2 pos)
    {
        var go = new GameObject("Button " + label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 100);
        go.AddComponent<Image>().color = new Color(0.9f, 0.45f, 0.15f);
        var button = go.AddComponent<Button>();
        NewText(go.transform, label, 50, new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta);
        return button;
    }

    static MenuController NewMenuController()
    {
        var go = new GameObject("MenuController");
        go.AddComponent<AudioSource>().playOnAwake = false;
        var menu = go.AddComponent<MenuController>();
        Set(menu, "clickSound", aClick);
        return menu;
    }

    static void CreateMainMenu()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        NewCameraAndLight(new Color(0.35f, 0.6f, 0.95f));
        NewMusic();
        var canvas = NewCanvas();
        var center = new Vector2(0.5f, 0.5f);
        NewText(canvas, "BLUEYPIXEL", 120, center, new Vector2(0, 250), new Vector2(1600, 200));
        NewText(canvas, "Mover: A/D o flechas   Saltar: Espacio", 40, center, new Vector2(0, 100), new Vector2(1600, 80));

        var menu = NewMenuController();
        UnityEventTools.AddStringPersistentListener(NewButton(canvas, "Jugar", new Vector2(0, -80)).onClick, menu.LoadScene, "Level1");
        UnityEventTools.AddVoidPersistentListener(NewButton(canvas, "Salir", new Vector2(0, -220)).onClick, menu.QuitGame);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{Root}/Scenes/MainMenu.unity");
    }

    static void CreateGameOver()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        NewCameraAndLight(new Color(0.15f, 0.12f, 0.25f));
        var canvas = NewCanvas();
        var result = NewText(canvas, "Game Over", 100, new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1600, 300));

        var menu = NewMenuController();
        Set(menu, "resultText", result);
        UnityEventTools.AddStringPersistentListener(NewButton(canvas, "Reintentar", new Vector2(0, -60)).onClick, menu.LoadScene, "Level1");
        UnityEventTools.AddStringPersistentListener(NewButton(canvas, "Menú", new Vector2(0, -200)).onClick, menu.LoadScene, "MainMenu");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{Root}/Scenes/GameOver.unity");
    }

    static void CreateLevel1()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = NewCameraAndLight(new Color(0.45f, 0.75f, 1f));
        NewMusic();

        // Grid con dos Tilemaps: uno con colisión (suelo) y otro decorativo (fondo)
        var grid = new GameObject("Grid").AddComponent<Grid>();
        var ground = NewTilemap(grid.transform, "Ground", 0);
        ground.gameObject.layer = GroundLayer;
        ground.gameObject.AddComponent<TilemapCollider2D>();
        var deco = NewTilemap(grid.transform, "Decoration", -5);

        GameObject player = null;
        var entities = new GameObject("Entities").transform;
        int width = 0;
        for (int row = 0; row < LevelMap.Length; row++)
        {
            string line = LevelMap[row];
            width = Mathf.Max(width, line.Length);
            int y = 5 - row;
            for (int x = 0; x < line.Length; x++)
            {
                var cell = new Vector3Int(x, y, 0);
                var pos = new Vector3(x + 0.5f, y + 0.5f, 0);
                switch (line[x])
                {
                    case '#':
                        bool top = row == 0 || x >= LevelMap[row - 1].Length || LevelMap[row - 1][x] != '#';
                        ground.SetTile(cell, top ? tGrass : tDirt); break;
                    case 'B': ground.SetTile(cell, tBrick); break;
                    case '~': deco.SetTile(cell, tCloud); break;
                    case '*': deco.SetTile(cell, tBush); break;
                    case 'c': Spawn(pCoin, pos, entities); break;
                    case 'E': Spawn(pEnemy, pos, entities); break;
                    case '^': Spawn(pSpike, pos, entities); break;
                    case 'F': Spawn(pGoal, new Vector3(pos.x, y + 1.5f, 0), entities); break;
                    case 'P': player = Spawn(pPlayer, pos, null); break;
                }
            }
        }

        // Zona de muerte bajo el nivel (si el jugador cae a un hoyo)
        var killZone = new GameObject("KillZone");
        killZone.transform.position = new Vector3(width / 2f, -10f, 0);
        var kz = killZone.AddComponent<BoxCollider2D>();
        kz.isTrigger = true; kz.size = new Vector2(width + 40, 2);
        killZone.AddComponent<Hazard>();

        // Cámara que sigue al jugador
        float halfWidth = cam.orthographicSize * 16f / 9f;
        cam.transform.position = new Vector3(halfWidth, 0, -10);
        var follow = cam.gameObject.AddComponent<CameraFollow>();
        Set(follow, "target", player.transform);
        Set(follow, "minX", halfWidth);
        Set(follow, "maxX", width - halfWidth);

        // UI de puntaje y GameManager
        var canvas = NewCanvas();
        var score = NewText(canvas, "Puntos: 0", 60, new Vector2(0, 1), new Vector2(40, -30), new Vector2(500, 100));
        score.alignment = TextAnchor.MiddleLeft;
        Set(new GameObject("GameManager").AddComponent<GameManager>(), "scoreText", score);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{Root}/Scenes/Level1.unity");
    }

    static Tilemap NewTilemap(Transform grid, string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid, false);
        var tm = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>().sortingOrder = order;
        return tm;
    }

    static GameObject Spawn(GameObject prefab, Vector3 pos, Transform parent)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        return go;
    }
}
