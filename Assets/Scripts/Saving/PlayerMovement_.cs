using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_ : MonoBehaviour
{
    [SerializeField] int speed;
    [SerializeField] int turnSpeed;


    // Start is called before the first frame update
    void Awake()
    {
        SaveManager.Init();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime);
        transform.Rotate(turnSpeed * Vector3.up * Input.GetAxis("Horizontal") * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))    
            Save();
        else if (Input.GetMouseButtonDown(1))   
            Load();
    }
    void Save()
    {
        // Instantiate & init my Save Data Structure.
        SaveObject saveObject = new SaveObject
        {
            SpeedPlayer = speed,
            turnSpeedPlayer = turnSpeed,
            playerPosition = transform.position,
            playerRotation = transform.eulerAngles
        };        
        // Transform the SaveObject Data to JSON format Data
        string json = JsonUtility.ToJson(saveObject);
        // Save the data
        SaveManager.Save(json);
        Debug.Log("aaa");
    }
    void Load()
    {
        // Load the data
        string saveString = SaveManager.Load();
        // Transform from JSON format Data to SaveObject Data
        if(saveString != null)
        {
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);
            transform.position = saveObject.playerPosition;
            transform.eulerAngles = saveObject.playerRotation;
            speed = saveObject.SpeedPlayer;
            turnSpeed = saveObject.turnSpeedPlayer;
        }
    }

    // I create a class which contains the data type I want to save
    private class SaveObject
    {
        public int SpeedPlayer;
        public int turnSpeedPlayer;
        public Vector3 playerPosition;
        public Vector3 playerRotation;
    }
}
