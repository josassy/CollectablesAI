using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

// public class GemArena : Area
public class GemArena : MonoBehaviour
{
  [Tooltip("The agents collecting gems")]
  public List<CollectorAgent> agents;

  [Tooltip("The gems to be collected")]
  private List<GameObject> gemList;

  [Tooltip("Gem prefabs to choose from")]
  public List<GameObject> prefabs;

  [Tooltip("The acceptable x range to spawn")]
  public float rangeX;

  [Tooltip("The acceptable z range to spawn")]
  public float rangeZ;

  private void Start()
  {
    ResetArea();
  }

  public void ResetArea()
  {
    RemoveAllGems();
    PlaceAgents();
    SpawnGems(8);
  }

  public void RemoveGem(GameObject gem)
  {
    gemList.Remove(gem);
    Destroy(gem);
    if (gemList.Count == 0)
    {
      ResetArea();
    }
  }

  public int GemsRemaining
  {
    get { return gemList.Count; }
  }

  public Vector3 ChooseRandomPosition(float height)
  {
    return transform.position + new Vector3(Random.Range(-rangeX / 2, rangeX / 2), height, Random.Range(-rangeZ / 2, rangeZ / 2));
  }

  private void RemoveAllGems()
  {
    if (gemList != null)
    {
      for (int i = 0; i < gemList.Count; i++)
      {
        if (gemList[i] != null)
        {
          Destroy(gemList[i]);
        }
      }
    }
    gemList = new List<GameObject>();
  }

  private void PlaceAgents()
  {
    foreach (CollectorAgent agent in agents)
    {
      Rigidbody rigidbody = agent.GetComponent<Rigidbody>();
      rigidbody.velocity = Vector3.zero;
      rigidbody.angularVelocity = Vector3.zero;
      agent.transform.position = ChooseRandomPosition(1.5f);
      agent.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }
  }

  private void SpawnGems(int count)
  {
    // spawn 8 gems
    for (int i = 0; i < count; i++)
    {
      // instantiate and configure gem
      GameObject gem = Instantiate<GameObject>(getPrefab(i).gameObject);
      AnimationScript script = gem.GetComponent<AnimationScript>();
      script.isFloating = false;
      script.isScaling = false;
      gem.tag = "gem";

      // add collisionmesh
      gem.AddComponent<MeshCollider>();

      // set scaling to half size
      gem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

      // attempt to position gem such that it does not collide with anything in the level
      bool positionOk = false;
      Vector3 newPosition = Vector3.zero;
      int numAttempts = 0;
      while (!positionOk && numAttempts < 5)
      {
        newPosition = ChooseRandomPosition(1f);
        // todo: check if new position collides with anything
        positionOk = true;
      }
      if (!positionOk)
      {
        Debug.Log($"Number of attempts for choosing gem position exceeded. Using position {newPosition}");
      }
      gem.transform.position = newPosition;

      // set parent transform
      gem.transform.SetParent(this.transform);

      // add to gemlist
      gemList.Add(gem);
    }
  }

  private GameObject getPrefab(int index)
  {
    // choose between 4 different meshes
    int meshIndex = index % prefabs.Count;
    return prefabs[meshIndex];
  }
}