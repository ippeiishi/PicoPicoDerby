// TipData.cs
using System;
using System.Collections.Generic;

// RemoteConfigから取得するJSON配列の各要素に対応
[Serializable]
public class TipEntry {
    public string category;
    public string title;
    public string description;
}

// UIで使いやすいように、カテゴリごとにグループ化されたデータ構造
public class CategorizedTips {
    public string categoryName;
    public List<TipEntry> tips;

    public CategorizedTips(string name) {
        categoryName = name;
        tips = new List<TipEntry>();
    }
}