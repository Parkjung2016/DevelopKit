using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Skddkkkk.DevelopKit.Editor
{
    public class DevelopKitHubWindow : EditorWindow
    {
        private const string basicTemplatePackageUrl = "https://github.com/Parkjung2016/DevelopKit_BasicTemplate.git";
        private const string basicTemplatePackageName = "com.skddkkkk.developkit.basictemplate";
        private const string frameworkPackageUrl = "https://github.com/Parkjung2016/DevelopKit_Framework.git";
        private const string frameworkPackageName = "com.skddkkkk.developkit.framework";
        private static readonly Vector2 windowSize = new Vector2(500, 500);
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        private Button installButton;

        private AddRequest addRequest;
        private RemoveRequest removeRequest;

        [MenuItem("Skddkkkk/DevelopKit Hub")]
        public static void ShowExample()
        {
            DevelopKitHubWindow wnd = GetWindow<DevelopKitHubWindow>();
            wnd.titleContent = new GUIContent("DevelopKit Hub");
            wnd.maxSize = windowSize + new Vector2(0.1f, 0.1f);
            wnd.minSize = windowSize;
        }

        public async void CreateGUI()
        {
            var installedPackageList = await GetInstalledPackages();
            var root = rootVisualElement;
            var uxml = m_VisualTreeAsset.Instantiate();
            
            var basicTemplateSection = uxml.Q("BasicTemplate");
            var frameworkeSection = uxml.Q("Framework");
            SetBasicTemplateInstallButton(installedPackageList, basicTemplateSection);
            SetFrameworkInstallButton(installedPackageList, frameworkeSection);
            uxml.Q("SolvePakcageDependencies").Q("Background").Q<Button>("Btn_Install").clicked += () => { };
            
            root.Add(uxml);
        }

        private void SetBasicTemplateInstallButton(PackageCollection installedPackageList,
            VisualElement basicTemplateSection)
        {
            var installBtn = basicTemplateSection.Q("Background").Q<Button>("Btn_Install");
            if (installedPackageList.Any(p => p.name == basicTemplatePackageName))
            {
                installBtn.text = "Remove Basic Template";
                basicTemplateSection.RemoveFromClassList("installable");
                basicTemplateSection.AddToClassList("removeable");
                installBtn.clicked += () =>
                {
                    installButton = installBtn;
                    RemovePackage(basicTemplatePackageName);
                };
            }
            else
            {
                installBtn.text = "Install Basic Template";
                basicTemplateSection.RemoveFromClassList("removeable");
                basicTemplateSection.AddToClassList("installable");
                installBtn.clicked += () =>
                {
                    installButton = installBtn;
                    InstallPackage(basicTemplatePackageUrl);
                };
            }
        }

        private void SetFrameworkInstallButton(PackageCollection installedPackageList,
            VisualElement frameworkSection)
        {
            var installBtn = frameworkSection.Q("Background").Q<Button>("Btn_Install");
            if (installedPackageList.Any(p => p.name == frameworkPackageName))
            {
                installBtn.text = "Remove Framework";
                frameworkSection.RemoveFromClassList("installable");
                frameworkSection.AddToClassList("removeable");
                installBtn.clicked += () =>
                {
                    installButton = installBtn;
                    RemovePackage(frameworkPackageName);
                };
            }
            else
            {
                installBtn.text = "Install Framework";
                frameworkSection.RemoveFromClassList("removeable");
                frameworkSection.AddToClassList("installable");
                installBtn.clicked += () =>
                {
                    installButton = installBtn;
                    InstallPackage(frameworkPackageUrl);
                };
            }
        }

        private void RemovePackage(string packageName)
        {
            installButton.SetEnabled(false);

            removeRequest = Client.Remove(packageName);
            EditorApplication.update += OnRemoveProgress;
        }

        private void OnRemoveProgress()
        {
            if (!removeRequest.IsCompleted)
                return;

            EditorUtility.ClearProgressBar();
            EditorApplication.update -= OnRemoveProgress;

            if (removeRequest.Status == StatusCode.Success)
            {
                EditorUtility.DisplayDialog(
                    "Remove Complete",
                    "패키지가 제거되었습니다.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Remove Failed",
                    removeRequest.Error.message,
                    "OK");
            }

            installButton.SetEnabled(true);
        }

        private void InstallPackage(string packageName)
        {
            installButton.SetEnabled(false);
            addRequest = Client.Add(packageName);
            EditorApplication.update += OnInstallProgress;
        }

        private void OnInstallProgress()
        {
            if (!addRequest.IsCompleted)
                return;
            EditorUtility.ClearProgressBar();
            EditorApplication.update -= OnInstallProgress;

            if (addRequest.Status != StatusCode.Success)
            {
                EditorUtility.DisplayDialog(
                    "Install Failed",
                    addRequest.Error.message,
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Install Complete",
                    $"패키지가 설치되었습니다.",
                    "OK");
            }
        }

        private async Task<PackageCollection> GetInstalledPackages()
        {
            var tcs = new TaskCompletionSource<PackageCollection>();
            ListRequest request = Client.List(true);

            EditorApplication.update += Progress;

            void Progress()
            {
                if (!request.IsCompleted)
                    return;

                EditorApplication.update -= Progress;
                if (request.Status == StatusCode.Success)
                {
                    tcs.SetResult(request.Result);
                }
                else
                {
                    tcs.SetException(new System.Exception(request.Error.message));
                }
            }

            return await tcs.Task;
        }
    }
}
