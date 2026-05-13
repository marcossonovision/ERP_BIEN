namespace Exporta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        

        private void btn_export_Click(object sender, EventArgs e)
        {
            const int MAX_LINES_PER_PART = 6000;

            try
            {
                // 1) Seleccionar carpeta raíz
                string rootPath;
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Selecciona la carpeta raíz";
                    if (folderDialog.ShowDialog() != DialogResult.OK)
                        return;

                    rootPath = folderDialog.SelectedPath;
                }

                // 2) Elegir "base" del nombre (solo para sacar carpeta + prefijo)
                //    Ejemplo: si eliges C:\Out\DumpSonovision.txt -> generará DumpSonovisionPart1.txt, Part2...
                string baseSavePath;
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Elige dónde guardar (se generarán Part1, Part2, ...)";
                    saveDialog.Filter = "Archivo de texto (*.txt)|*.txt";
                    saveDialog.FileName = "DumpSonovision.txt";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return;

                    baseSavePath = saveDialog.FileName;
                }

                string outDir = Path.GetDirectoryName(baseSavePath)!;
                string baseName = Path.GetFileNameWithoutExtension(baseSavePath);
                string prefix = Path.Combine(outDir, baseName + "Part"); // -> ...\DumpSonovisionPart

                // 3) Enumerar ficheros .cs y .cshtml (ignorando carpetas inaccesibles)
                var enumOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    ReturnSpecialDirectories = false,
                    AttributesToSkip = FileAttributes.ReparsePoint // evita bucles por symlinks/junctions
                };

                var files = Directory
                    .EnumerateFiles(rootPath, "*.*", enumOptions)
                    .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f)
                    .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show("No se han encontrado ficheros .cs o .cshtml.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 4) Escritura en partes con límite de líneas, SIN dividir ficheros
                int part = 1;
                int currentLines = 0;
                int partsGenerated = 0;

                StreamWriter? writer = null;
                string currentPartPath = "";

                void OpenNewPart()
                {
                    writer?.Dispose();

                    currentPartPath = $"{prefix}{part}.txt";
                    writer = new StreamWriter(currentPartPath, false, System.Text.Encoding.UTF8);

                    currentLines = 0;
                    partsGenerated++;
                }

                // Cabecera/Separadores por fichero (cuentan como líneas)
                // Ajusta si cambias el formato.
                int headerLines = 5; // separator + "FICHERO:" + path + separator + blank line
                int footerLines = 2; // 2 líneas en blanco tras el contenido

                OpenNewPart();

                int processed = 0;

                foreach (var file in files)
                {
                    // 4.1) Calcular cuántas líneas ocupará ESTE fichero (sin leerlo entero en memoria)
                    int fileContentLines;
                    bool canRead = true;

                    try
                    {
                        // Cuenta líneas recorriendo el fichero (stream)
                        fileContentLines = File.ReadLines(file).Count();
                    }
                    catch
                    {
                        // Si no se puede leer, lo tratamos como bloque de error (pocas líneas)
                        canRead = false;
                        fileContentLines = 3; // "ERROR..." + ruta + mensaje (aprox). Ajustaremos al escribir.
                    }

                    int blockLinesEstimate = headerLines + fileContentLines + footerLines;

                    // 4.2) Si al anexar este bloque se supera el máximo, empezamos una nueva parte,
                    //      PERO sin dividir este fichero: se va entero a la nueva parte.
                    //      (Si el bloque en sí supera 6000, se escribe igualmente en una parte vacía.)
                    if (currentLines > 0 && currentLines + blockLinesEstimate > MAX_LINES_PER_PART)
                    {
                        part++;
                        OpenNewPart();
                    }

                    // 4.3) Escribir cabecera del fichero
                    WriteLineCounted("=======================================================");
                    WriteLineCounted("FICHERO:");
                    WriteLineCounted(file);
                    WriteLineCounted("=======================================================");
                    WriteLineCounted("");

                    // 4.4) Escribir contenido o error
                    if (canRead)
                    {
                        try
                        {
                            foreach (var line in File.ReadLines(file))
                            {
                                WriteLineCounted(line);
                            }
                        }
                        catch (Exception exRead)
                        {
                            // Si falla en mitad, registramos error pero NO reventamos el proceso
                            WriteLineCounted("");
                            WriteLineCounted("### ERROR LEYENDO EL FICHERO (durante la lectura) ###");
                            WriteLineCounted(exRead.Message);
                        }
                    }
                    else
                    {
                        // Bloque de error si no pudimos ni contar/leer
                        WriteLineCounted("### ERROR LEYENDO EL FICHERO ###");
                        WriteLineCounted("No se ha podido acceder/leer este fichero.");
                        // Nota: ya hemos escrito la ruta arriba
                    }

                    // Footer (separación)
                    WriteLineCounted("");
                    WriteLineCounted("");

                    processed++;
                }

                writer?.Dispose();

                MessageBox.Show(
                    $"Exportación completada.\n" +
                    $"Ficheros procesados: {processed}\n" +
                    $"Partes generadas: {partsGenerated}\n\n" +
                    $"Salida: {prefix}1.txt ... {prefix}{partsGenerated}.txt",
                    "OK",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Local function para contar líneas escritas
                void WriteLineCounted(string text)
                {
                    writer!.WriteLine(text);
                    currentLines++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
