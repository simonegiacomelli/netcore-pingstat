using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Specialized;

/// <summary>
/// Created by Simone Giacomelli
/// http://www.simonegiacomelli.com
/// IniFile versione 1.5.2
/// </summary>
/// 

/*  12/08/2010  1.5.2 
 *              * fixed bug in ReadSection 
 * 
 *  09/08/2010  1.5.1 
 *              * fixed bug related to file change detection in private method save()
 * 
 *  27/03/2009  1.5.0
 *              *renamed method getSectionsList to ReadSectionsName
 *              *renamed method existSection to ExistSection
 *              *fixed bug in getSection
 *              *fixed bug in DeleteSection
 *  15/05/2008  1.4.8
 *              +added ToString() returns Filename
 *  28/02/2008  1.4.7
 *              *fixed bug in existSection
 *  21/01/2008  1.4.6
 *              +added property FileName
 *  19/01/2008  1.4.5
 *              +added ReadBoolAndForce
 *  11/01/2008  1.4.4 
 *              *fixed bug in parse()
 *  18/12/2007  1.4.3
 *              +added FileExists 
 *              +added ReadSection
 *  01/12/2007  1.4.2
 *              *Renamed method getSection to existSection
 *              +added method getSectionsList             
*/
public class IniFile : IniFileBase
{
    public IniFile(String filename)
        : base(filename)
    {

    }

    #region GESTIONE DATE TIME & DATETIME

    string datetimeFormat = "yyyy-MM-dd HH.mm.ss";
    public void WriteDateTime(string section, string key, DateTime value)
    {
        internalWriteDateTime(section, key, value, datetimeFormat);
    }
    public DateTime ReadDateTime(string section, string key, DateTime defaultValue)
    {
        return internalReadDateTime(section, key, defaultValue, datetimeFormat);
    }

    string dateFormat = "yyyy-MM-dd";
    public void WriteDate(string section, string key, DateTime value)
    {
        internalWriteDateTime(section, key, value, dateFormat);
    }
    public DateTime ReadDate(string section, string key, DateTime defaultValue)
    {
        return internalReadDateTime(section, key, defaultValue, dateFormat);
    }

    string timeFormat = "HH.mm.ss";
    public void WriteTime(string section, string key, DateTime value)
    {
        internalWriteDateTime(section, key, value, timeFormat);
    }
    public DateTime ReadTime(string section, string key, DateTime defaultValue)
    {
        return internalReadDateTime(section, key, defaultValue, timeFormat);
    }


    private DateTime internalReadDateTime(string section, string key, DateTime defaultValue, string format)
    {
        string d = ReadString(section, key, null);
        if (string.IsNullOrEmpty(d))
            return defaultValue;
        return DateTime.ParseExact(d, format, System.Globalization.CultureInfo.InvariantCulture);

    }

    private void internalWriteDateTime(string section, string key, DateTime value, string format)
    {
        WriteString(section, key, value.ToString(format));
    }
    #endregion

    #region GESTIONE BOOLEAN

    public void WriteBool(string section, string key, bool value)
    {
        WriteString(section, key, value.ToString());
    }
    public bool ReadBool(string section, string key, bool defaultValue)
    {
        string d = ReadString(section, key, null);
        if (string.IsNullOrEmpty(d))
            return defaultValue;
        return Boolean.Parse(d);
    }

    #endregion

    #region GESTIONE INTEGER

    public void WriteInteger(string section, string key, int value)
    {
        WriteString(section, key, value.ToString());
    }

    public int ReadInteger(string section, string key, int defaultValue)
    {
        string d = ReadString(section, key, null);
        if (string.IsNullOrEmpty(d))
            return defaultValue;
        return Int32.Parse(d);
    }

    #endregion

    /// <summary>
    /// Restitiusce il valore associato alla key nella sezione specificata; se il valore
    /// non esiste lo imposta al valore di default
    /// </summary>
    /// <param name="section">La sezione dell'ini</param>
    /// <param name="key">la chiave nella sezione</param>
    /// <param name="defaultString">valore di default, se la chiave non esiste viene scritta con questo valore</param>
    /// <returns></returns>
    public string ReadStringAndForce(string section, string key, string defaultString)
    {
        string res = ReadString(section, key, null);
        if (res == null)
        {
            WriteString(section, key, defaultString);
            res = defaultString;
        }
        return res;
    }
    
    /// <summary>
    /// Restitiusce il valore associato alla key nella sezione specificata; se il valore
    /// non esiste lo imposta al valore di default
    /// </summary>
    /// <param name="section">La sezione dell'ini</param>
    /// <param name="key">la chiave nella sezione</param>
    /// <param name="defaultString">valore di default, se la chiave non esiste viene scritta con questo valore</param>
    /// <returns></returns>
    public bool ReadBoolAndForce(string section, string key, bool defaultValue)
    {
        string res = ReadString(section, key, null);
        if (res == null)
        {
            WriteBool(section, key, defaultValue);
            return defaultValue;
        }
        return ReadBool(section,key,defaultValue);
    }


    /// <summary>
    /// Restitiusce il valore associato alla key nella sezione specificata; se il valore
    /// non esiste lo imposta al valore di default
    /// </summary>
    /// <param name="section">La sezione dell'ini</param>
    /// <param name="key">la chiave nella sezione</param>
    /// <param name="defaultString">valore di default, se la chiave non esiste viene scritta con questo valore</param>
    /// <returns></returns>
    public int ReadIntegerAndForce(string section, string key, int defaultValue)
    {
        string res = ReadString(section, key, null);
        if (res == null)
        {
            WriteInteger(section, key, defaultValue);
            return defaultValue;
        }
        return ReadInteger(section, key, defaultValue);
    }
}
public class IniFileBase
{

    class Section
    {
        public string sectionName;
        public int startLineNumber; //of [section]
        public int lastUsedLine;
        public int lastLine;
        public override string ToString()
        {
            return sectionName.Replace("[", "").Replace("]", "");
        }
    }
    class Entry
    {
        public string getCombined() { return section.sectionName + "," + key; }
        public string key;
        public string value;
        public int lineNumber;
        public int equalPositionPlus1;
        public Section section;
        public override string ToString()
        {
            return getCombined() + "=" + value;
        }
    }

    private Dictionary<string, Entry> values = new Dictionary<string, Entry>();
    private Dictionary<int, Entry> valuesByLineNr = new Dictionary<int, Entry>();
    /// <summary>
    /// association between section name and section instance
    /// </summary>
    private Dictionary<string, Section> sections = new Dictionary<string, Section>();
    private List<string> lines = new List<string>();


    private FileInfo inifile;
    private DateTime inifileDate;
    public IniFileBase(String filename)
    {
         setInifile(filename);
    }
    private void setInifile(string filename)
    {
        this.inifile = new FileInfo(filename);
        load();
        parse();
    }
    public string FileName
    {
        get { return inifile.FullName; }
        set { setInifile(value) ; }
    }
    /// <summary>
    /// If the section already exists, gets the Section instance. 
    /// If it doesn't, creates a new Section instance using sectionName and startLine 
    /// </summary>
    /// <param name="sectionName">section name with brackets [section]</param>
    /// <param name="startLine">starting line count of section</param>
    /// <returns></returns>
    Section getSection(string sectionName, int startLine)
    {
        if (sections.ContainsKey(sectionName.ToLower()))
            return sections[sectionName.ToLower()];

        Section s = new Section();
        s.sectionName = sectionName;
        s.startLineNumber = startLine;
        s.lastUsedLine = startLine;
        s.lastLine = startLine;
        sections.Add(sectionName.ToLower(), s);
        return s;
    }

    public List<string> ReadSectionsName()
    {
        ifFileChangedReload();
        List<string> res = new List<string>();
        foreach (Section sect in sections.Values)
        {
            string sectName = sect.sectionName;
            sectName = sectName.Remove(sectName.Length - 1, 1);
            sectName = sectName.Remove(0, 1);
            if (sectName != "")
                res.Add(sectName);
        }
        
        return res;
    }
    public bool ExistSection(string sectionName)
    {
        ifFileChangedReload();
        string sec = "[" + sectionName.ToLower() + "]";
        if (sections.ContainsKey(sec))
            return true;

        return false;
    }
    public bool FileExists
    {
        get { return inifile.Exists; }
    }
    private void load()
    {
        lines.Clear();
        if (!inifile.Exists)
            return;
        inifileDate = inifile.LastWriteTime;
        StreamReader reader = inifile.OpenText();

        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            lines.Add(line);
        }

        reader.Close();
    }
    private void parse()
    {
        sections.Clear();
        values.Clear();
        valuesByLineNr.Clear();

        Section currentSection = getSection("[]", lines.Count - 1);

        for (int idx = 0; idx < lines.Count; idx++)
        {

            string line = lines[idx];
            //cerco di identificare la sezione
            if (line.Trim().StartsWith("[") && line.Trim().EndsWith("]"))
            {
                currentSection = getSection(line, idx);
                continue;
            }
            currentSection.lastLine = idx;
            if (line.Trim().StartsWith(";"))
                continue;
            //cerco di identificare una string con chiave=valore
            if (line.IndexOf("=") < 0)
                continue;

            Entry entry = createEntry(currentSection, line);
            entry.lineNumber = idx;

            addEntryToList(entry);
            currentSection.lastUsedLine = idx;
        }



    }
    Entry createEntry(Section section, string line)
    {
        Entry entry = new Entry();
        entry.section = section;

        int equalPos = line.IndexOf('=');
        entry.key = line.Substring(0, equalPos).Trim();
        entry.equalPositionPlus1 = equalPos + 1;
        entry.value = line.Substring(equalPos + 1);
        return entry;
    }
    void addEntryToList(Entry entry)
    {
        string key = entry.getCombined().ToLower();
        if (values.ContainsKey(key))
            return;
        values.Add(key, entry);
        valuesByLineNr.Add(entry.lineNumber, entry);
    }
    /// <summary>
    /// get Entry with this section and key pair
    /// </summary>
    /// <param name="section">section name with brackets [section]</param>
    /// <param name="key">key name</param>
    /// <returns></returns>
    private Entry getEntry(string section, string key)
    {
        string combined = (section + "," + key).ToLower();
        if (values.ContainsKey(combined))
            return values[combined];
        return null;
    }
    public bool DeleteSection(string section)
    {
        ifFileChangedReload();
        if (!ExistSection(section))
            return false;
        Section sectionInstance = getSection("[" + section + "]", -1);
        if (sectionInstance.startLineNumber == -1)
            return false;

        for (int i = sectionInstance.lastLine; i >= sectionInstance.startLineNumber; i--)
        {
            lines.RemoveAt(i);
        }
        save();
        parse();
        return true;
    }
    public bool DeleteString(string section, string key)
    {
        ifFileChangedReload();
        Entry entry = getEntry("[" + section + "]", key);
        if (entry != null)
        {
            lines.RemoveAt(entry.lineNumber);
            parse();
            save();
            return true;
        } return false;

    }
    public string ReadString(string section, string key, string defaultString)
    {
        ifFileChangedReload();
        Entry entry = getEntry("[" + section + "]", key);
        if (entry != null)
            return entry.value;
        return defaultString;
    }
    public void WriteString(string section, string key, string value)
    {
        ifFileChangedReload();
        string sectionWithBrackets = "[" + section + "]";
        //verifico se questa chiave esiste gia'
        Entry entry = getEntry(sectionWithBrackets, key);
        if (entry != null)
        {
            entry.value = value;
            string oldline = lines[entry.lineNumber];
            string newline = oldline.Substring(0, entry.equalPositionPlus1) + value;
            lines[entry.lineNumber] = newline;
            save();
        }
        else
        {
            //verifico se la sezione esiste gia'

            Section sectionInstance = getSection(sectionWithBrackets, -1);
            if (sectionInstance.startLineNumber == -1)
            {
                int padding = 2;
                int c= lines.Count;
                if ( c> 1)
                {
                    if (lines[c - 1] == "")
                        padding--;
                    if (lines[c - 2] == "")
                        padding--;
                }
                
                while((padding--)>0)
                    lines.Add("");
                
                lines.Add(sectionWithBrackets);
                sectionInstance.startLineNumber = sectionInstance.lastUsedLine = lines.Count - 1;
            }

            string line = key + "=" + value;
            sectionInstance.lastUsedLine++;
            lines.Insert(sectionInstance.lastUsedLine, line);


            entry = createEntry(sectionInstance, line);
            entry.lineNumber = sectionInstance.lastUsedLine;
            addEntryToList(entry);
            parse();
            save();
        }
    }

    private void save()
    {
        var oldlast = inifile.LastWriteTime;
        StreamWriter writer = new StreamWriter(inifile.Open(inifile.Exists ? FileMode.Truncate : FileMode.Create, FileAccess.Write, FileShare.Write));
        try
        {
            foreach (string line in lines)
                writer.WriteLine(line);
        }
        finally
        {
            writer.Close();
            if (oldlast == inifile.LastWriteTime)
            {
                inifile.LastWriteTime = inifile.LastWriteTime.AddTicks(1);
            }
            inifileDate = inifile.LastWriteTime;
        }

    }
    protected void ifFileChangedReload()
    {
        inifile.Refresh();
        if (inifileDate != inifile.LastWriteTime)
        {
            load();
            parse();
        }
    }
    public NameValueCollection ReadSection(string section, NameValueCollection keyValue)
    {
        ifFileChangedReload();

        if( keyValue == null)
            keyValue = new NameValueCollection();

        Section s ;
        
        if( sections.TryGetValue(bracketize(section),out s) )
        {
            for(int idx=s.startLineNumber ;idx<=s.lastLine ;idx++)
            {
                Entry e;
                if (valuesByLineNr.TryGetValue(idx, out e))
                {
                    keyValue.Add(e.key, e.value);
                }
            }
        }
        return keyValue;
    }

    private string bracketize(string section)
    {
        return "[" + section.ToLower() + "]";
    }

    public override string ToString()
    {
        return FileName;
    }

}


