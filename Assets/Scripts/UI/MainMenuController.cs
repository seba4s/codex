using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnNuevaPartida()
    {
        SceneTransition.Instance.CargarEscena("T01_Materializacion");
    }

    public void OnReanudar()
    {
        SceneTransition.Instance.CargarEscena("EscogerPartida");
    }

    public void OnMejoras()
    {
        SceneTransition.Instance.CargarEscena("Mejoras");
    }

    public void OnOpciones()
    {
        SceneTransition.Instance.CargarEscena("Opciones");
    }

    public void OnSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
