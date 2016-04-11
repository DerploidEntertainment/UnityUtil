using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Rolling {

    public enum AreaType {
        Box,
        Circle
    }

    // Strech out the image as you need in the sprite render, then the following script
    // will generate a nice set of repeated, correctly oriented sprites inside its bounds
    [RequireComponent(typeof(MeshRenderer))]
    public class TiledSprite : MonoBehaviour {

        // INSPECTOR FIELDS
        public Sprite TileSprite;
        public int TileAmount = 10;
        public AreaType AreaType = AreaType.Box;
        public int CircleVertices = 10;

        // EVENT HANDLERS
        private void Awake() {
            createRenderer();
            //createChildRenderers();
        }

        private void createRenderer() {
            // Define a new Mesh filling the appropriate area
            Mesh mesh = null;
            switch (AreaType) {
                case AreaType.Box:
                    mesh = createBoxMesh();
                    break;

                case AreaType.Circle:
                    mesh = createCircleMesh();
                    break;
            }

            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;
        }
        private Mesh createBoxMesh() {
            Mesh mesh = new Mesh();

            Vector2 pos = transform.position;
            Vector2 objSize = transform.lossyScale;
            Vector2 tileSize = TileSprite.bounds.size;
            Vector2 halfGlobal = transform.lossyScale / 2f;
            Vector2 halfLocal = transform.localScale / 2f;
            Vector2 numV = new Vector2(Mathf.RoundToInt(objSize.x / tileSize.x) + 1, Mathf.RoundToInt(objSize.y / tileSize.y) + 1);

            // Define the vertices in and around the box
            Vector3[,] vertices = new Vector3[(int)numV.x, (int)numV.y];
            for (int x = 0; x < numV.x; x++) {
                for (int y = 0; y < numV.y; y++) {
                    vertices[x, y] = new Vector2((x - pos.x) / objSize.x, (y - pos.y) / objSize.y);
                }
            }
            mesh.vertices = vertices.Cast<Vector3>().ToArray();

            // Define the triangles of the box
            List<int> tris = new List<int>(2 * (int)(numV.x * numV.y));
            for (int x = 0; x < numV.x - 1; x++) {
                for (int y = 0; y < numV.y - 1; y++) {
                    tris.Add((x + 0) * (int)numV.y + (y + 0));
                    tris.Add((x + 0) * (int)numV.y + (y + 1));
                    tris.Add((x + 1) * (int)numV.y + (y + 1));

                    tris.Add((x + 1) * (int)numV.y + (y + 1));
                    tris.Add((x + 1) * (int)numV.y + (y + 0));
                    tris.Add((x + 0) * (int)numV.y + (y + 0));
                }
            }
            mesh.triangles = tris.ToArray();

            // Define texture UVs for the vertices
            Vector2[,] uvs = new Vector2[(int)numV.x, (int)numV.y];
            for (int x = 0; x < numV.x; x++) {
                for (int y = 0; y < numV.y; y++) {
                    float percentX = Mathf.InverseLerp((pos.x - halfGlobal.x) / objSize.x, (pos.x + halfGlobal.x) / objSize.x, vertices[x,y].x);
                    float percentY = Mathf.InverseLerp((pos.y - halfGlobal.y) / objSize.y, (pos.y + halfGlobal.y) / objSize.y, vertices[x,y].y);
                    uvs[x,y] = new Vector2(percentX, percentY) * TileAmount;
                }
            }
            mesh.uv = uvs.Cast<Vector2>().ToArray();

            return mesh;
        }
        private Mesh createCircleMesh() {
            Mesh mesh = new Mesh();

            return mesh;
        }
        private void createChildRenderers() {

            // Cache some values needed for looping
            Vector2 objSize = transform.localScale;
            Vector2 tileSize = TileSprite.bounds.size;
            Vector2 tileExtents = TileSprite.bounds.extents;
            Vector2 tilePivot = new Vector2(TileSprite.pivot.x / TileSprite.textureRect.width, TileSprite.pivot.y / TileSprite.textureRect.height);
            Vector2 count = new Vector2(Mathf.RoundToInt(objSize.x / tileSize.x), Mathf.RoundToInt(objSize.y / tileSize.y));

            // Loop through and spit out repeated tiles
            Vector2 tileScale = new Vector2(1f / objSize.x, 1f / objSize.y);
            Vector2 corner = transform.position - new Vector3(Mathf.FloorToInt(objSize.x / 2f), Mathf.FloorToInt(objSize.y / 2f));
            for (int x = 0; x < count.x; x++) {
                for (int y = 0; y < count.y; y++) {
                    string name = string.Format("Tile_{0},{1}", x, y);
                    GameObject tile = new GameObject(name);

                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = TileSprite;

                    Transform tileTrans = tile.transform;
                    Vector2 pos = new Vector2((x * tileSize.x + tilePivot.x) / objSize.x, (y * tileSize.y + tilePivot.y) / objSize.y);
                    tileTrans.parent = transform;
                    tileTrans.localPosition = pos - tileExtents;
                    tileTrans.localScale = tileScale;
                    tileTrans.localRotation = Quaternion.identity;
                }
            }
        }

        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }

}
