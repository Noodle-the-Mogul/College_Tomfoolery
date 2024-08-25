using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cutscene : MonoBehaviour
{
    public float changeTime;
    public string sceneName;
    void Start()
    {
        // Reset time scale to normal when a new scene is loaded
        Time.timeScale = 1f;
    }
    // Update is called once per frame
    void Update()
    {   changeTime-=Time.deltaTime;
        if(changeTime<=0){
            SceneManager.LoadScene(sceneName);
        }
    }
}
