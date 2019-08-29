﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadInitialMap : MonoBehaviour {

    [SerializeField] Transform interfaceTransform; //For detecting 4+ lane maps
    [SerializeField] Transform noteGridScalingOffset; //For detecting 4+ lane maps
    [SerializeField] AudioTimeSyncController atsc;
    [Space]
    [SerializeField] NotesContainer notesContainer;
    [SerializeField] ObstaclesContainer obstaclesContainer;
    [SerializeField] EventsContainer eventsContainer;
    [SerializeField] BPMChangesContainer bpmContainer;
    [Space]
    [Tooltip("TODO: Monstercat and Imagine Dragons environments. Vapor Frame/Big Mirror v2 temporary.")]
    [SerializeField] GameObject[] PlatformPrefabs;

    public static Action<PlatformDescriptor> PlatformLoadedEvent;

    private BeatSaberMap map;
    private BeatSaberSong.DifficultyBeatmap data;
    private BeatSaberSong song;

    void Awake()
    {
        //StartCoroutine(LoadMap());
        SceneTransitionManager.Instance.AddLoadRoutine(LoadMap());
    }

    public IEnumerator LoadMap()
    {
        if (BeatSaberSongContainer.Instance == null) yield break;
        song = BeatSaberSongContainer.Instance.song;
        float offset = 0;
        int environmentID = 0;
        environmentID = SongInfoEditUI.GetEnvironmentIDFromString(song.environmentName);
        if (song.customData != null && (song.customData["_customEnvironment"] != null || song.customData["_customEnvironment"].Value != ""))
        {
            //Custom Platforms code here, maybe. But for now, temporarily load Vapor Frame/Big Mirror V2.
            switch (song.customData["_customEnvironment"].Value.ToLower())
            {
                case "vapor frame": environmentID = 7; break;
                case "big mirror v2": environmentID = 8; break;
                case "dueling dragons": environmentID = 9; break;
                case "collider": environmentID = 10; break;
            }
        }
        GameObject platform = PlatformPrefabs[environmentID];
        yield return Instantiate(platform, new Vector3(0, -0.5f, -1.5f), Quaternion.identity);
        PlatformDescriptor descriptor = Resources.FindObjectsOfTypeAll<PlatformDescriptor>().First(); //SHHHHH
        BeatmapEventContainer.ModifyTypeMode = descriptor.SortMode;
        PlatformLoadedEvent.Invoke(descriptor);
        descriptor.RedColor = BeatSaberSong.Cleanse(BeatSaberSongContainer.Instance.difficultyData.colorLeft);
        descriptor.BlueColor = BeatSaberSong.Cleanse(BeatSaberSongContainer.Instance.difficultyData.colorRight);
        try {

            map = BeatSaberSongContainer.Instance.map;
            data = BeatSaberSongContainer.Instance.difficultyData;
            offset = (song.beatsPerMinute / 60) * (data.offset / 1000);
            int noteLaneSize = 2; //Half of it, anyways
            int noteLayerSize = 3;
            if (map != null) {
                foreach (BeatmapNote noteData in map._notes) {
                    BeatmapNoteContainer beatmapNote = notesContainer.SpawnObject(noteData) as BeatmapNoteContainer;
                    if (noteData._lineIndex >= 1000 || noteData._lineIndex <= -1000 || noteData._lineLayer >= 1000 || noteData._lineLayer <= -1000) continue;
                    if (2 - noteData._lineIndex > noteLaneSize) noteLaneSize = 2 - noteData._lineIndex;
                    if (noteData._lineIndex - 1 > noteLaneSize) noteLaneSize = noteData._lineIndex - 1;
                    if (noteData._lineLayer + 1 > noteLayerSize) noteLayerSize = noteData._lineLayer + 1;
                }
                foreach (BeatmapObstacle obstacleData in map._obstacles)
                {
                    BeatmapObstacleContainer beatmapObstacle = obstaclesContainer.SpawnObject(obstacleData) as BeatmapObstacleContainer;
                    if (obstacleData._lineIndex >= 1000 || obstacleData._lineIndex <= -1000) continue;
                    if (2 - obstacleData._lineIndex > noteLaneSize) noteLaneSize = 2 - obstacleData._lineIndex;
                    if (obstacleData._lineIndex - 1 > noteLaneSize) noteLaneSize = obstacleData._lineIndex - 1;
                }
                foreach (MapEvent eventData in map._events) eventsContainer.SpawnObject(eventData);
                foreach (BeatmapBPMChange bpmData in map._BPMChanges) bpmContainer.SpawnObject(bpmData);
                notesContainer.SortObjects();
                obstaclesContainer.SortObjects();
                eventsContainer.SortObjects();
                bpmContainer.SortObjects();
                noteGridScalingOffset.localScale = new Vector3((float)(noteLaneSize * 2) / 10 + 0.01f, 1, noteGridScalingOffset.localScale.z);
                interfaceTransform.localScale = new Vector3((float)(noteLaneSize * 2) / 10 + 0.01f, 1, (float)noteLayerSize + 0.1f);
            }

        } catch (Exception e) {
            Debug.LogWarning("No mapping for you!");
            Debug.LogException(e);
        }
    }
}
