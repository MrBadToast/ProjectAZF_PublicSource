using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization.Reporting;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
#endif

public class LocalizationUtil : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Localization/스트링 시트 업데이트")]
    public static void PullAllExtensions()
    {
        // Get every String Table Collection
        var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();

        foreach (var collection in stringTableCollections)
        {
            // Its possible a String Table Collection may have more than one GoogleSheetsExtension.
            // For example if each Locale we pushed/pulled from a different sheet.
            foreach (var extension in collection.Extensions)
            {
                if (extension is GoogleSheetsExtension googleExtension)
                {
                    PullExtension(googleExtension);
                }
            }
        }
    }

    [MenuItem("Localization/스트링 시트 열기")]
    public static void OpenStringsheet()
    {
        Application.OpenURL("https://docs.google.com/spreadsheets/d/1uf_wCp78DPIQCohxQaXe5ALdWE079ACYRNRv2URG6lk/edit?usp=sharing");
    }

    static void PullExtension(GoogleSheetsExtension googleExtension)
    {
        // Setup the connection to Google
        var googleSheets = new GoogleSheets(googleExtension.SheetsServiceProvider);
        googleSheets.SpreadSheetId = googleExtension.SpreadsheetId;

        // Now update the collection. We can pass in an optional ProgressBarReporter so that we can updates in the Editor.
        googleSheets.PullIntoStringTableCollection(googleExtension.SheetId, googleExtension.TargetCollection as StringTableCollection, googleExtension.Columns, reporter: new ProgressBarReporter());
    }
#endif
}

