using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] public PieceType pieceType = PieceType.None;
    [SerializeField] private int pieceID = -1;
    [SerializeField] public bool playerPiece = false;
    [SerializeField] public bool isDead = false;
    [SerializeField] public Vector2 tileCoordinates = Vector2.zero;
    [SerializeField] public float pieceValue = 0.0f;
    [HideInInspector] public Vector3 lastPosition = Vector3.zero;

    [SerializeField] private SpriteRenderer SR = null;
    [SerializeField] private Sprite rankSprite = null;
    [SerializeField] private Sprite unknownSprite = null;

    private bool debugOn = false;

    private void Start()
    {
        if (!playerPiece && !debugOn) SR.sprite = unknownSprite;
        else SR.sprite = rankSprite;

        lastPosition = transform.position;
    }

    public void ChangeSprite()
    {
        debugOn = !debugOn;

        if (!playerPiece && !debugOn) SR.sprite = unknownSprite;
        else SR.sprite = rankSprite;
    }
}