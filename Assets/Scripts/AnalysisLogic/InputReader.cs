using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using SQLiter;
using System.Collections.Generic;
using System;
using System.Threading;
using CellexalVR.Menu.SubMenus;
using CellexalVR.General;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using System.Drawing;
using System.Diagnostics;
using CellexalVR.AnalysisLogic.H5reader;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// A class for reading data files and creating objects in the virtual environment.
    /// 
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject spatialGraphPrefab;
        public int facsGraphCounter;
        public bool attributeFileRead;
        public AttributeReader attributeReader;

        private readonly char[] separators = {' ', '\t'};
        private CellManager cellManager;
        private SQLite database;
        private SelectionManager selectionManager;
        private ColorByIndexMenu indexMenu;
        private GraphFromMarkersMenu createFromMarkerMenu;
        private MDSReader mdsReader;
        private NetworkReader networkReader;

        private GameObject headset;

        //private StatusDisplay status;
        //private StatusDisplay statusDisplayHUD;
        //private StatusDisplay statusDisplayFar;
        private GraphGenerator graphGenerator;
        private string currentPath;
        private Bitmap image1;
        public Dictionary<string, H5Reader> h5readers;

        [Tooltip("Automatically loads the Bertie dataset")]
        public bool debug;
        //Flag for loading previous sessions

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            h5readers = new Dictionary<string, H5Reader>();
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            cellManager = referenceManager.cellManager;
            database = referenceManager.database;
            selectionManager = referenceManager.selectionManager;
            indexMenu = referenceManager.indexMenu;
            createFromMarkerMenu = referenceManager.createFromMarkerMenu;
            if (CrossSceneInformation.Spectator)
            {
                headset = referenceManager.spectatorRig;
                referenceManager.headset = headset;
                headset.SetActive(true);
            }
            else
            {
                headset = referenceManager.headset;
                referenceManager.spectatorRig.SetActive(false);
            }

            graphGenerator = referenceManager.graphGenerator;
            currentPath = "";
            facsGraphCounter = 0;
            RScriptRunner.SetReferenceManager(referenceManager);
            // CellexalEvents.UsernameChanged.AddListener(LoadPreviousGroupings);
        }


        [ConsoleCommand("inputReader", folder: "Data", aliases: new string[] {"readfolder", "rf"})]
        public void ReadFolderConsole(string path)
        {
            referenceManager.multiuserMessageSender.SendMessageReadFolder(path);
            ReadFolder(path);
        }

        /// <summary>
        /// Reads one folder of data and creates the graphs described by the data.
        /// </summary>
        /// 
        /// <param name="path">path to the file</param>
        private void ReadFileH5(string path, Dictionary<string, string> h5config)
        {
            bool confExists = Directory.EnumerateFiles("Data\\" + path, "*.conf").Any();
            if (!confExists)
            {
                if (h5config == null)
                {
                    referenceManager.h5ReaderAnnotatorScriptManager.AddAnnotator(path);
                    return;
                }
            }

            string fullPath = Directory.GetCurrentDirectory() + "\\Data\\" + path;
            GameObject go = new GameObject(path);
            H5Reader h5Reader = go.AddComponent<H5Reader>();
            h5readers.Add(path, h5Reader);
            h5Reader.SetConf(path, h5config);
            StartCoroutine(h5Reader.H5ReadGraphs(fullPath));
            if (PhotonNetwork.isMasterClient)
            {
                referenceManager.configManager.MultiUserSynchronise();
            }
        }

        /// <summary>
        /// Reads one folder of data and creates the graphs described by the data.
        /// </summary>
        /// <param name="path"> The path to the folder. </param>
        //[ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readfolder", "rf" })]
        public void ReadFolder(string path, Dictionary<string, string> h5config = null)
        {
            currentPath = path;
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = workingDirectory + "\\Data\\" + path;
            if (Directory.Exists(workingDirectory + "\\Output"))
            {
                if (File.Exists(workingDirectory + "\\Output\\r_log.txt"))
                {
                    CellexalLog.Log("Deleting old r log file");
                    File.Delete(workingDirectory + "\\Output\\r_log.txt");
                }
            }

            UpdateSelectionToolHandler();
            attributeFileRead = false;
            CellexalLog.Log("Started reading the data folder at " + CellexalLog.FixFilePath(fullPath));
            CellexalUser.DataSourceFolder = currentPath;
            selectionManager.DataDir = fullPath;
            if (!debug)
            {
                // clear the network folder
                string networkDirectory = (CellexalUser.UserSpecificFolder + "\\Resources\\Networks").FixFilePath();
                if (!Directory.Exists(networkDirectory))
                {
                    CellexalLog.Log("Creating directory " + CellexalLog.FixFilePath(networkDirectory));
                    Directory.CreateDirectory(networkDirectory);
                }

                string[] networkFilesList = Directory.GetFiles(networkDirectory, "*");
                CellexalLog.Log("Deleting " + networkFilesList.Length + " files in " +
                                CellexalLog.FixFilePath(networkDirectory));
                foreach (string f in networkFilesList)
                {
                    File.Delete(f);
                }
            }

            //summertwerk
            bool h5 = Directory.EnumerateFiles("Data\\" + path, "*.h5ad").Any();
            bool loom = Directory.EnumerateFiles("Data\\" + path, "*.loom").Any();
            if (h5 || loom)
            {
                ReadFileH5(path, h5config);
                return;
            }

            database.InitDatabase(fullPath + "\\database.sqlite");
            string[] mdsFiles = Directory.GetFiles(fullPath,
                CrossSceneInformation.Tutorial ? "DDRTree.mds" : "*.mds");

            if (mdsFiles.Length == 0)
            {
                CellexalError.SpawnError("Empty dataset",
                    "The loaded dataset did not contain any .mds files. Make sure you have placed the dataset files in the correct folder.");
                throw new System.InvalidOperationException("Empty dataset");
            }

            CellexalLog.Log("Reading " + mdsFiles.Length + " .mds files");
            mdsReader = gameObject.AddComponent<MDSReader>();
            mdsReader.referenceManager = referenceManager;
            StartCoroutine(mdsReader.ReadMDSFiles(fullPath, mdsFiles));
            graphGenerator.isCreating = true;

            // multiple_exp if (currentPath.Length > 0)
            // multiple_exp {
            // multiple_exp     currentPath += "+" + path;
            // multiple_exp }
            //LoadPreviousGroupings();
            //string[] spatialMds = Directory.GetFiles(fullPath, "*.spatialmds");
            //StartCoroutine(ReadSpatialMDSFiles(fullPath, spatialMds));
            if (PhotonNetwork.isMasterClient)
            {
                referenceManager.configManager.MultiUserSynchronise();
            }
        }

        private void UpdateSelectionToolHandler()
        {
            referenceManager.heatmapGenerator.selectionManager = referenceManager.selectionManager;
            referenceManager.networkGenerator.selectionManager = referenceManager.selectionManager;
            referenceManager.graphManager.selectionManager = referenceManager.selectionManager;
        }

        /// <summary>
        /// Reads a facs marker file.
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <param name="file">The path to the file.</param>
        public void ReadGraphFromMarkerFile(string path, string file)
        {
            facsGraphCounter++;
            StartCoroutine(mdsReader.ReadMDSFiles(path, new string[] {file}, GraphGenerator.GraphType.FACS, false));
        }

        public void ReadFilterFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.fil");
            referenceManager.filterMenu.CreateButtons(files);
        }

        /// <summary>
        /// Start the R session that will run in the background. 
        /// </summary>
        /// <param name="serverType">If you are running several sessions give a serverType name that works as a prefix so the 
        /// R session knows which file to look for.</param>
        /// <returns></returns>
        public IEnumerator StartServer(string serverType)
        {
            Process currentProcess = Process.GetCurrentProcess();
            int pid = currentProcess.Id;
            string rScriptFilePath = Application.streamingAssetsPath + @"\R\start_server.R";
            string serverName = CellexalUser.UserSpecificFolder + "\\" + serverType + "Server";
            string dataSourceFolder = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder;
            string args = serverName + " " + dataSourceFolder + " " +
                          CellexalUser.UserSpecificFolder + " " + pid;
            CellexalLog.Log("Running start server script at " + rScriptFilePath + " with the arguments " + args);
            string value = null;
            Thread t = new Thread(
                () => { value = RScriptRunner.RunFromCmd(rScriptFilePath, args, true); });
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            t.Start();
            while (!File.Exists(serverName + ".pid"))
            {
                if (value != null && !value.Equals(string.Empty))
                {
                    CellexalError.SpawnError("Failed to start R Server",
                        "Make sure you have set the correct R path in the launcher menu");
                    yield break;
                }

                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("Start Server finished in " + stopwatch.Elapsed.ToString());
            referenceManager.notificationManager.SpawnNotification(serverType + " R Server Session Initiated.");
            StartCoroutine(referenceManager.reportManager.LogStart());
        }

        /// <summary>
        /// To clean up server files after termination.
        /// Can be called if the user wants to start a new session (e.g. when loading a new dataset) or when exiting the program. 
        /// </summary>
        public void QuitServer()
        {
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.pid");
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.input.R");

            List<string> h5ReadersToRemove = new List<string>();

            foreach (KeyValuePair<string, H5Reader> kvp in referenceManager.inputReader.h5readers)
            {
                kvp.Value.CloseConnection();
                Destroy(kvp.Value.gameObject);
                h5ReadersToRemove.Add(kvp.Key);
            }

            foreach (string reader in h5ReadersToRemove)
                h5readers.Remove(reader);

            //File.Delete(CellexalUser.UserSpecificFolder + "\\geneServer.pid");
            CellexalLog.Log("Stopped Server");
        }

        /// <summary>
        /// Reads the index.facs file.
        /// </summary>
        public void ReadFacsFiles(string path, int nbrOfCells)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string fullPath = path + "\\index.facs";

            if (!File.Exists(fullPath))
            {
                print("File " + fullPath + " not found");
                CellexalLog.Log(".facs file not found");
                return;
            }

            FileStream fileStream = new FileStream(fullPath, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);

            // The file format should be:
            //             TYPE_1  TYPE_2 ...
            // CELLNAME_1  VALUE   VALUE  
            // CELLNAME_2  VALUE   VALUE
            // ...

            string headerLine = streamReader.ReadLine();
            if (headerLine == null)
            {
                // empty file
                CellexalLog.Log("Empty index.facs file");
                return;
            }

            string[] header = headerLine.Split(new string[] {"\t", " "}, StringSplitOptions.RemoveEmptyEntries);
            float[] min = new float[header.Length];
            float[] max = new float[header.Length];
            string[] values = new string[header.Length + 1];
            int i = 0;
            for (; i < min.Length; ++i)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            for (i = 0; !streamReader.EndOfStream; ++i)
            {
                string line = streamReader.ReadLine();
                SplitValues(line, ref values, separators);
                string cellName = values[0];
                for (int j = 0; j < values.Length - 1; ++j)
                {
                    float value = float.Parse(values[j + 1],
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    if (value < min[j])
                        min[j] = value;
                    if (value > max[j])
                        max[j] = value;
                }

                for (int j = 0; j < values.Length - 1; ++j)
                {
                    // normalize to the range [0, 29]
                    cellManager.AddFacs(cellName, header[j],
                        float.Parse(values[j + 1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    cellManager.AddFacsValue(cellName, header[j], values[j + 1]);
                }
            }

            // set the min and max values
            for (i = 0; i < header.Length; ++i)
            {
                cellManager.FacsRanges[header[i].ToLower()] = new Tuple<float, float>(min[i], max[i]);
            }

            streamReader.Close();
            fileStream.Close();
            indexMenu.CreateButtons(header);
            createFromMarkerMenu.CreateButtons(header);
            cellManager.Facs = header;
            CellexalLog.Log("Successfully read " + CellexalLog.FixFilePath(fullPath));
            stopwatch.Stop();
        }

        private void SplitValues(string line, ref string[] values, char[] separators)
        {
            int charIndex = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                int nextSeparator = line.IndexOfAny(separators, charIndex);
                if (nextSeparator >= 0)
                {
                    values[i] = line.Substring(charIndex, nextSeparator - charIndex);
                }
                else
                {
                    values[i] = line.Substring(charIndex);
                }

                charIndex = nextSeparator + 1;
            }
        }

        /// <summary>
        /// Reads the files containg networks.
        /// </summary>
        public void ReadNetworkFiles(int layoutSeed)
        {
            networkReader = gameObject.AddComponent<NetworkReader>();
            networkReader.referenceManager = referenceManager;
            StartCoroutine(networkReader.ReadNetworkFilesCoroutine(layoutSeed));
        }


        /// <summary>
        /// Read all the user.group files which cointains the grouping information from previous sessions.
        /// </summary>
        public void LoadPreviousGroupings()
        {
            string dataFolder = CellexalUser.UserSpecificFolder;
            string groupingsInfoFile = dataFolder + "\\groupings_info.txt";
            CellexalLog.Log("Started reading the previous groupings files");
            //print(groupingsInfoFile);
            if (!File.Exists(groupingsInfoFile))
            {
                CellexalLog.Log(
                    "WARNING: No groupings info file found at " + CellexalLog.FixFilePath(groupingsInfoFile));
                return;
            }

            FileStream fileStream = new FileStream(groupingsInfoFile, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            // skip the header
            List<string> groupingNames = new List<string>();
            List<int> fileLengths = new List<int>();
            string line = "";
            string[] words = null;
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                if (line == "") continue;
                words = line.Split(new char[] {'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);

                // set the grouping's name to [the grouping's number]\n[number of colors in grouping]\n[number of cells in groupings]
                string groupingName = words[0];
                int indexOfLastDot = groupingName.LastIndexOf(".");
                if (indexOfLastDot == -1)
                {
                    CellexalLog.Log("WARNING: Could not find \'.\' in \"" + words[0] + "\"");
                    indexOfLastDot = groupingName.Length - 1;
                }

                //string groupingNumber = groupingName.Substring(indexOfLastDot, groupingName.Length - indexOfLastDot);
                groupingNames.Add(groupingName + "\n" + words[1] + "\n" + words[2]);
                fileLengths.Add(int.Parse(words[2]));
            }

            streamReader.Close();
            fileStream.Close();

            CellexalLog.Log("Reading " + groupingNames.Count + " files");
            // initialize the arrays
            string[][] cellNames = new string[groupingNames.Count][];
            int[][] groups = new int[groupingNames.Count][];
            string[] graphNames = new string[groupingNames.Count];
            Dictionary<int, UnityEngine.Color>[] groupingColors =
                new Dictionary<int, UnityEngine.Color>[groupingNames.Count];
            for (int i = 0; i < cellNames.Length; ++i)
            {
                cellNames[i] = new string[fileLengths[i]];
                groups[i] = new int[fileLengths[i]];
            }

            for (int i = 0; i < groupingNames.Count; ++i)
            {
                groupingColors[i] = new Dictionary<int, UnityEngine.Color>();
            }

            words = null;
            string[] files = Directory.GetFiles(dataFolder, "selection*.txt");
            for (int i = 0; i < fileLengths.Count; ++i)
            {
                string file = files[i];
                fileStream = new FileStream(file, FileMode.Open);
                streamReader = new StreamReader(fileStream);

                for (int j = 0; j < fileLengths[i]; ++j)
                {
                    line = streamReader.ReadLine();
                    print(line);
                    words = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                    cellNames[i][j] = words[0];

                    try
                    {
                        int group = int.Parse(words[3]);
                        groups[i][j] = group;
                        UnityEngine.Color groupColor;
                        ColorUtility.TryParseHtmlString(words[1], out groupColor);
                        groupingColors[i][group] = groupColor;
                    }
                    catch (FormatException e)
                    {
                        foreach (string s in words)
                        {
                            print(s);
                        }

                        print(words[3] + " " + file + " " + j + "\n" + e.StackTrace);
                        streamReader.Close();
                        fileStream.Close();
                        return;
                    }
                }

                graphNames[i] = words[2];
                streamReader.Close();
                fileStream.Close();
            }

            referenceManager.selectionFromPreviousMenu.SelectionFromPreviousButton(graphNames, groupingNames.ToArray(),
                cellNames, groups, groupingColors);
            CellexalLog.Log("Successfully read " + groupingNames.Count + " files");
        }

        public void ReadAnnotationFile(string path)
        {
            //string dataFolder = CellexalUser.UserSpecificFolder;
            if (!File.Exists(path))
            {
                CellexalLog.Log("Could not find file:" + path);
                return;
            }

            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            List<Cell> cellsToAnnotate = new List<Cell>();
            Graph graph = referenceManager.graphManager.Graphs[0];
            int numPointsAdded = 0;
            string line = streamReader.ReadLine();
            string[] words = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            string firstCellName = words[0];
            cellsToAnnotate.Add(referenceManager.cellManager.GetCell(firstCellName));
            string annotation = words[1];
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                words = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (words[1] != annotation)
                {
                    referenceManager.annotationManager.AddAnnotation(annotation, cellsToAnnotate, path);
                    cellsToAnnotate.Clear();
                    annotation = words[1];
                }

                cellsToAnnotate.Add(referenceManager.cellManager.GetCell(words[0]));
                numPointsAdded++;
            }

            referenceManager.annotationManager.AddAnnotation(annotation, cellsToAnnotate, path);
            cellsToAnnotate.Clear();
            CellexalLog.Log(string.Format("Added {0} points to annotation", numPointsAdded));
            CellexalEvents.CommandFinished.Invoke(true);
            streamReader.Close();
            fileStream.Close();
        }

        public void ReadSelectionFile(string path)
        {
            //string dataFolder = CellexalUser.UserSpecificFolder;
            if (!File.Exists(path))
            {
                CellexalLog.Log("Could not find file:" + path);
                return;
            }
            
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            SelectionManager selectionManager = referenceManager.selectionManager;
            selectionManager.CancelSelection();
            GraphManager graphManager = referenceManager.graphManager;
            int numPointsAdded = 0;
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string[] words = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                int group;
                UnityEngine.Color groupColor;

                try
                {
                    group = int.Parse(words[3]);
                    ColorUtility.TryParseHtmlString(words[1], out groupColor);
                    if (referenceManager.selectionToolCollider.Colors.Contains(groupColor))
                    {
                        referenceManager.settingsMenu.AddSelectionColor(groupColor);
                    }
                }
                catch (FormatException)
                {
                    CellexalLog.Log(string.Format("Bad color on line {0} in file {1}.", numPointsAdded + 1,
                        path));
                    streamReader.Close();
                    fileStream.Close();
                    CellexalEvents.CommandFinished.Invoke(false);
                    return;
                }

                selectionManager.AddGraphpointToSelection(graphManager.FindGraphPoint(words[2], words[0]), group, false,
                    groupColor);
                numPointsAdded++;
            }

            CellexalLog.Log(string.Format("Added {0} points to selection", numPointsAdded));
            CellexalEvents.CommandFinished.Invoke(true);
            streamReader.Close();
            fileStream.Close();
        }


        [ConsoleCommand("inputReader", aliases: new string[] {"selectfromprevious", "sfp"})]
        public void ReadAndSelectPreviousSelection(int index)
        {
            string dataFolder = CellexalUser.UserSpecificFolder;
            string[] files = Directory.GetFiles(dataFolder, "selection*.txt");
            if (files.Length == 0)
            {
                CellexalLog.Log("No previous selections found.");
                CellexalEvents.CommandFinished.Invoke(false);
                return;
            }
            else if (index < 0 || index >= files.Length)
            {
                CellexalLog.Log(string.Format(
                    "Index \'{0}\' is not within the range [0, {1}] when reading previous selection files.", index,
                    files.Length - 1));
                CellexalEvents.CommandFinished.Invoke(false);
                return;
            }

            string path = files[index];
            ReadSelectionFile(path);
        }
    }
}