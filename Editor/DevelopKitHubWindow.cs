using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

public class DevelopKitHubWindow : EditorWindow
{
    private const string basicTemplatePackageName = "https://github.com/Parkjung2016/Toolkit.git#basic-template";
    private static readonly Vector2 windowSize = new Vector2(500, 500);
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private AddRequest addRequest;
    [MenuItem("Parkjung2016/DevelopKit Hub")]
    public static void ShowExample()
    {
        DevelopKitHubWindow wnd = GetWindow<DevelopKitHubWindow>();
        wnd.titleContent = new GUIContent("DevelopKit Hub");
        wnd.maxSize = windowSize + new Vector2(0.1f, 0.1f);
        wnd.minSize = windowSize;
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        var uxml = m_VisualTreeAsset.Instantiate();

        root.Add(uxml);


        uxml.Q("BasicTemplate").Q("Background").Q<Button>("Btn_Install").clicked += () =>
        {
            InstallPackage(basicTemplatePackageName);
        };

        uxml.Q("SolvePakcageDependencies").Q("Background").Q<Button>("Btn_Install").clicked += () =>
        {
        };
    }


    void InstallPackage(string packageName)
    {
        EditorUtility.DisplayProgressBar(
              "Installing Package",
              $"Installing {packageName}...",
              0.3f);

        addRequest = Client.Add(packageName);
        EditorApplication.update += () =>
        {
            if (!addRequest.IsCompleted)
                return;

            EditorApplication.update -= OnPackageProgress;
        };
    }

    void OnPackageProgress()
    {
        if (!addRequest.IsCompleted)
            return;

        EditorUtility.ClearProgressBar();

        if (addRequest.Status == StatusCode.Success)
        {
            EditorUtility.DisplayDialog(
                "Install Complete",
                "패키지 설치가 완료되었습니다.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Install Failed",
                addRequest.Error.message,
                "OK");
        }

        EditorApplication.update -= OnPackageProgress;
    }
}
