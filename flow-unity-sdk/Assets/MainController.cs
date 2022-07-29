using System.Collections;
using System.Threading.Tasks;
using Blocto.Flow.Client.Http.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    private Button _button;
    
    private InputField _resultTxt;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private void Awake()
    {
        Debug.Log("Start Debug.");
        var tmp = GameObject.Find("Button");
        _button = tmp.GetComponent<Button>();
        _button.onClick.AddListener(ButtonClick);
        
        tmp = GameObject.Find("ResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
        
        _flowWebRequest = new FlowUnityWebRequest("https://rest-testnet.onflow.org/v1", this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    private async void ButtonClick()
    {
        // StartCoroutine(GetBlock());
        var id = await GetBlock();
        _resultTxt.text = id;
    }
    
    private async Task<string> GetBlock()
    {
        var result = _flowWebRequest.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        return result.Header.Id;
    }
}