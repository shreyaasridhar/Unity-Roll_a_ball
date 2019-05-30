using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Text countText;
    public Text winText;

    //Timer to show in game
    public Text counterText; //time
    public float seconds, minutes;

    private Rigidbody rb;
    private int count;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winText.text = "";
        counterText.text = "";
    }

    void FixedUpdate() 
    {
        float moveVertical = Input.GetAxis("Vertical") ;
        float moveHorizontal = Input.GetAxis("Horizontal") ;

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce (movement * speed) ;
    }

    void Update()
    {
        if (count < 13)
        {
            minutes = (int)(Time.time / 60f);
            seconds = (int)(Time.time % 60f);
            counterText.text = "Time:  " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

    }
    // Destroy everything that enters the trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false);
            count += 1;
            SetCountText();
        }

    }
    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 13)
            winText.text = "You Win!!! " + "Complete " + counterText.text;
    }
}
