﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongList : MonoBehaviour {

    [SerializeField]
    InputField searchField;

    [SerializeField]
    SongListItem[] items;

    [SerializeField]
    TMP_Text pageText;

    [SerializeField]
    int currentPage = 0;

    [SerializeField]
    int maxPage = 0;

    [SerializeField]
    List<BeatSaberSong> songs = new List<BeatSaberSong>();

    [SerializeField]
    Button firstButton;
    [SerializeField]
    Button prevButton;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    Button lastButton;
    [SerializeField]
    TextMeshProUGUI songLocationToggleText;

    public bool WIPLevels = true;

    private void Start() {
        //TODO remove
        Settings.BeatSaberInstallation = PlayerPrefs.GetString("install");
        RefreshSongList();
    }

    public void ToggleSongLocation()
    {
        WIPLevels = !WIPLevels;
        RefreshSongList();
        songLocationToggleText.text = WIPLevels ? "Custom\nLevels" : "Custom\nWIP\nLevels";
    }

    public void RefreshSongList() {
        //TODO get songs
        string[] directories = new string[] { };
        if (WIPLevels) //Grabs songs from CustomWIPLevels or CustomLevels
        {
            if (searchField.text == "") directories = Directory.GetDirectories(Settings.CustomWIPSongsFolder);
            else directories = Directory.GetDirectories(Settings.CustomWIPSongsFolder, searchField.text);
        }
        else
        {
            if (searchField.text == "") directories = Directory.GetDirectories(Settings.CustomSongsFolder);
            else directories = Directory.GetDirectories(Settings.CustomSongsFolder, searchField.text);
        }
        songs.Clear();
        for (int i = 0; i < directories.Length; i++) {
            BeatSaberSong song = BeatSaberSong.GetSongFromFolder(directories[i]);
            if (song == null)
            {   //Using ModSaber One-Click Install (Or 100K Playlist) puts songs inside another folder. This makes sure those gets loaded.
                string[] subDirectories = Directory.GetDirectories(directories[i]);
                for (int e = 0; e < subDirectories.Length; e++) song = BeatSaberSong.GetSongFromFolder(subDirectories[e]);
            }
            if (song != null) songs.Add(song);
        }
        songs = songs.OrderBy(x => x.songName).ToList(); //Add some sorting, maybe?
        if (items.Length > 10) //For some reason when refreshing again, items.Length is set to 15 instead of 10. This fixes that.
        {
            Stack<SongListItem> listitems = new Stack<SongListItem>(items.AsEnumerable());
            while (listitems.Count > 10) listitems.Pop();
            items = listitems.ToArray();
        }
        Debug.Log(songs.Count + "/" + items.Count());
        //maxPage = Mathf.Max((songs.Count - 1) / items.Length, 1);
        maxPage = Mathf.Max(0, Mathf.CeilToInt(songs.Count / items.Length));
        SetPage(0);
    }

    public void SetPage(int page) {
        if (page < 0 || page > maxPage) return;
        currentPage = page;
        LoadPage();
        pageText.text = "Page: " + (currentPage + 1) + "/" + (maxPage + 1);

        firstButton.interactable = currentPage != 0;
        prevButton.interactable = currentPage - 1 >= 0;
        nextButton.interactable = currentPage + 1 <= maxPage;
        lastButton.interactable = currentPage != maxPage;
    }

    public void LoadPage() {
        int offset = currentPage * items.Length;
        for (int i = 0; i < items.Length; i++) { // && (i + offset) < songs.Count; i++) {
            if (items[i] == null) continue;
            if (i + offset < songs.Count) {
                items[i].gameObject.SetActive(true);
                items[i].SetDisplayName(songs[i + offset].songName);
                items[i].AssignButton(songs[i + offset]);
            } else {
                items[i].gameObject.SetActive(false);
            }
        }
    }

    public void FirstPage() {
        SetPage(0);
    }

    public void PrevPage() {
        SetPage(currentPage - 1);
    }

    public void NextPage() {
        SetPage(currentPage + 1);
    }

    public void LastPage() {
        SetPage(maxPage);
    }

}