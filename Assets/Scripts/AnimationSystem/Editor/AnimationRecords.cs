using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecords : ScriptableObject
{
    public AnimationRecord[] Records;
    private Dictionary<Object, AnimationRecord> mRecordMaps = new Dictionary<Object, AnimationRecord>();
    public void Init()
    {
        if (Records==null)
        {
            return;
        }
        mRecordMaps = new Dictionary<Object, AnimationRecord>();
        for (int i = 0; i < Records.Length; i++)
        {
            AnimationRecord record = Records[i];
            if (record.Model==null)
            {
                continue;
            }
            mRecordMaps.Add(record.Model, record);
        }
    }
    public void AddRecord(GameObject model,AnimationRecord record)
    {
        if (model==null)
        {
            return;
        }
        if (record==null)
        {
            return;
        }
        if (mRecordMaps.ContainsKey(model))
        {
            mRecordMaps.Remove(model);
        }
        mRecordMaps.Add(model,record);
        syncRecords();
    }
    public bool RemoveRecord(GameObject model)
    {
        if (model==null)
        {
            return false;
        }
        bool result = mRecordMaps.Remove(model);
        syncRecords();
        return result;
    }
    public AnimationRecord GetRecord(GameObject model)
    {
        AnimationRecord record;
        mRecordMaps.TryGetValue(model, out record);
        return record;
    }
    private void syncRecords()
    {
        Records = new AnimationRecord[mRecordMaps.Count];
        int index = 0;
        foreach (var item in mRecordMaps)
        {
            Records[index] = item.Value;
            index++;
        }
    }
}
