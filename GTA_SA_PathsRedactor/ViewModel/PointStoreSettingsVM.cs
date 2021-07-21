using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PointStoreSettingsVM : Core.Entity
    {
        private TreeNodeWithItem? m_currentLoader;
        private TreeNodeWithItem? m_currentSaver;

        private ObservableCollection<TreeNodeWithItem> m_loaders;
        private ObservableCollection<TreeNodeWithItem> m_savers;

        private RelayCommand m_loadAssemlyCommand;

        public PointStoreSettingsVM()
        {
            m_loaders = new ObservableCollection<TreeNodeWithItem>();
            m_savers = new ObservableCollection<TreeNodeWithItem>();

            m_loadAssemlyCommand = new RelayCommand(LoadAssemblyCommandHandler);

            LoadDefaultInfo();
            InitialiazeAssembliesInfos();
        }

        public TreeNodeWithItem? CurrentLoader
        {
            get => m_currentLoader;
            set
            {
                m_currentLoader = value;
                OnPropertyChanged();
            }
        }
        public TreeNodeWithItem? CurrentSaver
        {
            get => m_currentSaver;
            set
            {
                m_currentSaver = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TreeNodeWithItem> Loaders => m_loaders;
        public ObservableCollection<TreeNodeWithItem> Savers => m_savers;

        public RelayCommand LoadAssemlyCommand => m_loadAssemlyCommand;

        private TreeNodeWithItem? GetExistAssemblyNode(AssemblyInfo assemblyInfo, IEnumerable<TreeNodeWithItem> treeNodeWithItems)
        {
            foreach (var node in treeNodeWithItems)
            {
                if (node.Element is not AssemblyInfo)
                    continue;

                if ((AssemblyInfo)node.Element == assemblyInfo)
                    return node;
            }

            return null;
        }

        private void InitialiazeAssembliesInfos()
        {
            foreach (var assembly in ProxyController.Assemblies)
            {
                LoadAssemblyInfo(assembly.GetAssemblyInfo(), assembly.Location);
            }
        }

        private void LoadAssemblyInfo(AssemblyInfo assemblyInfo, string assemblyLocation)
        {
            var jsonFileInfo = assemblyLocation.Substring(0, assemblyLocation.LastIndexOf('.')) + ".json";

            if (File.Exists(jsonFileInfo))
            {
                var derivedLoaders = ProxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, typeof(Core.PointLoader))
                                                    .Select(type => type.FullName);
                var derivedSavers = ProxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, typeof(Core.PointSaver))
                                                   .Select(type => type.FullName);

                ParseCustomInfo(File.ReadAllText(jsonFileInfo),
                                (loaders, savers) =>
                                {
                                    var joinedLoaders = loaders.Join(derivedLoaders,
                                                                     outer => outer.Name,
                                                                     inner => inner,
                                                                     (outer, inner) => outer);
                                    var joinedSavers = savers.Join(derivedSavers,
                                                                   outer => outer.Name,
                                                                   inner => inner,
                                                                   (outer, inner) => outer);

                                    var resultLoaders = derivedLoaders.Except(joinedLoaders.Select(jLoader => jLoader.Name))
                                                                      .Select(dLoader => new ItemInfo(dLoader, string.Empty, string.Empty))
                                                                      .Union(joinedLoaders);
                                    var resultSavers = derivedSavers.Except(joinedSavers.Select(jSaver => jSaver.Name))
                                                                    .Select(dSaver => new ItemInfo(dSaver, string.Empty, string.Empty))
                                                                    .Union(joinedSavers);

                                    var loadersNode = new TreeNodeWithItem(assemblyInfo);
                                    var saversNode = new TreeNodeWithItem(assemblyInfo);

                                    loadersNode.DisplayMember = saversNode.DisplayMember = "Title";
                                    saversNode.ValueMember = loadersNode.ValueMember = "FullName";

                                    FillNode(loadersNode, resultLoaders, "Name");
                                    FillNode(saversNode, resultSavers, "Name");

                                    if (resultLoaders.Any())
                                        m_loaders.Add(loadersNode);

                                    if (resultSavers.Any())
                                        m_savers.Add(saversNode);
                                });
            }
            else
            {
                var loaderNode = CreateNodeFromAssembly(assemblyInfo, typeof(Core.PointLoader));
                var saverNode = CreateNodeFromAssembly(assemblyInfo, typeof(Core.PointSaver));

                saverNode.DisplayMember = loaderNode.DisplayMember = "Title";
                saverNode.ValueMember = loaderNode.ValueMember = "FullName";

                m_loaders.Add(loaderNode);
                m_savers.Add(saverNode);
            }
        }

        private void LoadDefaultInfo()
        {
            var fileSource = new string(Encoding.UTF8.GetChars(AppResources.DefaultInfo));

            ParseCustomInfo(fileSource, (loaders, savers) => 
                            {
                                var currentAssembly = this.GetType().Assembly.GetAssemblyInfo();
                                var defaultInfo = new AssemblyInfo("Default", currentAssembly.FullName, currentAssembly.Version,
                                                                   currentAssembly.Author, currentAssembly.Company, currentAssembly.Product,
                                                                   "Default saver and loader for using program.");

                                var loadersNode = new TreeNodeWithItem(defaultInfo);
                                var saversNode = new TreeNodeWithItem(defaultInfo);

                                loadersNode.DisplayMember = saversNode.DisplayMember = "Title";
                                saversNode.ValueMember = loadersNode.ValueMember = "FullName";

                                foreach (var loader in loaders)
                                {
                                    var loaderNode = new TreeNodeWithItem(loader, "Name");
                                    loaderNode.ValueMember = "Name";

                                    loadersNode.Nodes.Add(loaderNode);
                                }
                                foreach (var saver in savers)
                                {
                                    var saverNode = new TreeNodeWithItem(saver, "Name");
                                    saverNode.ValueMember = "Name";

                                    saversNode.Nodes.Add(saverNode);
                                }

                                CurrentLoader = (TreeNodeWithItem)loadersNode.Nodes[0];
                                CurrentSaver = (TreeNodeWithItem)saversNode.Nodes[0];

                                m_loaders.Add(loadersNode);
                                m_savers.Add(saversNode);
                            });
        }
        private void FillNode<T>(TreeNodeWithItem node, IEnumerable<T> infos, string infoDisplayMember)
        {
            foreach (var info in infos)
            {
                var _node = new TreeNodeWithItem(info);
                _node.DisplayMember = infoDisplayMember;
                _node.ValueMember = infoDisplayMember;

                node.Nodes.Add(_node);
            }
        }

        private TreeNodeWithItem CreateNodeFromAssembly(AssemblyInfo assemblyInfo, Type baseType)
        {
            var derivedTypes = ProxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, baseType);
            var topmostNode = new TreeNodeWithItem(assemblyInfo);

            foreach (var type in derivedTypes)
            {
                var node = new TreeNodeWithItem(new ItemInfo(type.FullName, string.Empty, string.Empty));

                topmostNode.Nodes.Add(node);
                node.DisplayMember = "Name";
                node.ValueMember = "Name";
            }

            return topmostNode;
        }

        private void LoadAssemblyCommandHandler(object? obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dynamic-link library files (*.dll)|*.dll";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                var assemblyInfo = ProxyController.AddAssembly(fileName)?.GetAssemblyInfo();

                var existSaverNode = GetExistAssemblyNode(assemblyInfo, m_savers);
                var existLoaderNode = GetExistAssemblyNode(assemblyInfo, m_loaders);

                if (existSaverNode != null || existLoaderNode != null)
                {
                    CurrentSaver = (TreeNodeWithItem)existSaverNode?.Nodes.FirstOrDefault();
                    CurrentLoader = (TreeNodeWithItem)existLoaderNode?.Nodes.FirstOrDefault();
                    return;
                }

                LoadAssemblyInfo(assemblyInfo, fileName);
            }
        }

        private void ParseCustomInfo(string jsonString, Action<IEnumerable<ItemInfo?>, IEnumerable<ItemInfo?>> endAction)
        {
            var result = JsonConvert.DeserializeObject(jsonString);

            if (result is JObject jObj)
            {
                Func<JToken, ItemInfo> transformToken = token =>
                    {
                        var name = token.Value<string>("Name");
                        var description = token.Value<string>("Description");
                        var purpose = token.Value<string>("Purpose");

                        return new ItemInfo(name, purpose, description);
                    };

                var loaders = jObj.GetValue("Loaders")
                                  .Select(transformToken);
                var savers = jObj.GetValue("Savers")
                                 .Select(transformToken);

                endAction(loaders, savers);
            }
            else
            {
                throw new InvalidOperationException("Cannot parse info file.");
            }
        }
    }
}
