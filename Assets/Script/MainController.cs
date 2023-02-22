using Blocto.Sdk.Core.Extension;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    private Button _evmBtn;
    
    private Button _solanaBtn;
    
    private Button _flowBtn;

    public void Awake()
    {
        "In MainController.".ToLog();
        var tmp = GameObject.Find("EvmBtn");
        _evmBtn = tmp.GetComponent<Button>();
        _evmBtn.onClick.AddListener(EvmBtnOnClick);
        
        tmp = GameObject.Find("SolanaBtn");
        _solanaBtn = tmp.GetComponent<Button>();
        _solanaBtn.onClick.AddListener(SolanaBtnOnClick);
        
        tmp = GameObject.Find("FlowBtn");
        _solanaBtn = tmp.GetComponent<Button>();
        _solanaBtn.onClick.AddListener(FlowBtnOnClick);
    }
    
    public void EvmBtnOnClick()
    {
        $"On EvmBtn on click.".ToLog();
       SceneManager.LoadScene("EvmScene");
    }
    
    public void SolanaBtnOnClick()
    {
        $"On EvmBtn on click.".ToLog();
       SceneManager.LoadScene("SolanaScene");
    }
    
    public void FlowBtnOnClick()
    {
        $"On EvmBtn on click.".ToLog();
       SceneManager.LoadScene("FlowScene");
    }
}
