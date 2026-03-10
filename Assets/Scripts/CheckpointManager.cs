using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheckpointManager : MonoBehaviour
{
    [Header("Respawn Root")]
    [SerializeField] private Transform respawnRoot;
    [SerializeField] private Rigidbody2D[] bodiesToReset;
    [SerializeField] private Rigidbody2D[] fallCheckBodies;
    [SerializeField] private SpringJoint2D[] springsToReset;

    [Header("Checkpoint")]
    [SerializeField] private Transform initialCheckpoint;

    [Header("Auto Respawn")]
    [SerializeField] private bool autoRespawnWhenBelowY = false;
    [SerializeField] private float fallYThreshold = -10f;
    [SerializeField] private bool forceDynamicOnRespawn = true;

    [Header("Manual Respawn")]
    [SerializeField] private bool manualRespawnByKey = true;
    [SerializeField] private Key manualRespawnKey = Key.R;

    public static CheckpointManager Instance { get; private set; }

    private Vector3 _checkpointPosition;
    private Quaternion _rootStartRotation;
    private Vector3[] _bodyLocalPositions;
    private Quaternion[] _bodyLocalRotations;
    private Vector3[] _bodyLocalScales;
    private Transform[] _bodyOriginalParents;
    private float[] _springDistances;
    private float[] _springFrequencies;
    private float[] _springDampingRatios;
    private bool[] _springEnabledStates;
    private bool _hasCachedDefaults;

    private void Awake()
    {
        Instance = this;

        if (respawnRoot == null)
        {
            Debug.LogWarning("CheckpointManager has no respawn root assigned.");
            return;
        }

        _rootStartRotation = respawnRoot.rotation;
        _checkpointPosition = initialCheckpoint != null ? initialCheckpoint.position : respawnRoot.position;

        if (bodiesToReset == null || bodiesToReset.Length == 0)
        {
            bodiesToReset = CollectBodies(respawnRoot);
        }

        if (fallCheckBodies == null || fallCheckBodies.Length == 0)
        {
            fallCheckBodies = bodiesToReset;
        }

    }

    private void Start()
    {
        CacheDefaultsFromCurrentHierarchy();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        bool keyboardRespawn = kb != null && kb[manualRespawnKey].wasPressedThisFrame;
        bool gamepadRespawn = false;
        var pads = Gamepad.all;
        for (int i = 0; i < pads.Count; i++)
        {
            if (pads[i] != null && pads[i].dpad.up.wasPressedThisFrame)
            {
                gamepadRespawn = true;
                break;
            }
        }

        if (manualRespawnByKey && (keyboardRespawn || gamepadRespawn))
        {
            Respawn();
            return;
        }

        if (!autoRespawnWhenBelowY || fallCheckBodies == null)
        {
            return;
        }

        for (int i = 0; i < fallCheckBodies.Length; i++)
        {
            Rigidbody2D rb = fallCheckBodies[i];
            if (rb == null)
            {
                continue;
            }

            if (rb.position.y < fallYThreshold)
            {
                Respawn();
                return;
            }
        }
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        _checkpointPosition = checkpointPosition;
    }

    [ContextMenu("Respawn Now")]
    public void Respawn()
    {
        if (respawnRoot == null)
        {
            return;
        }

        if (!_hasCachedDefaults)
        {
            CacheDefaultsFromCurrentHierarchy();
        }

        respawnRoot.position = _checkpointPosition;
        respawnRoot.rotation = _rootStartRotation;

        RestoreBodyLocalState();
        RestoreSpringState();
        ResetBodies();
    }

    [ContextMenu("Recache Respawn Defaults")]
    public void CacheDefaultsFromCurrentHierarchy()
    {
        CacheBodyLocalState();
        CacheSpringState();
        _hasCachedDefaults = true;
    }

    private void ResetBodies()
    {
        if (bodiesToReset == null)
        {
            return;
        }

        for (int i = 0; i < bodiesToReset.Length; i++)
        {
            Rigidbody2D rb = bodiesToReset[i];

            if (rb == null)
            {
                continue;
            }

            if (forceDynamicOnRespawn)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }
    }

    private static Rigidbody2D[] CollectBodies(Transform root)
    {
        HashSet<Rigidbody2D> result = new HashSet<Rigidbody2D>();
        if (root == null)
        {
            return new Rigidbody2D[0];
        }

        Rigidbody2D[] found = root.GetComponentsInChildren<Rigidbody2D>(true);
        for (int j = 0; j < found.Length; j++)
        {
            result.Add(found[j]);
        }

        Rigidbody2D[] bodies = new Rigidbody2D[result.Count];
        result.CopyTo(bodies);
        return bodies;
    }

    private void CacheBodyLocalState()
    {
        if (bodiesToReset == null)
        {
            _bodyLocalPositions = null;
            _bodyLocalRotations = null;
            return;
        }

        _bodyLocalPositions = new Vector3[bodiesToReset.Length];
        _bodyLocalRotations = new Quaternion[bodiesToReset.Length];
        _bodyLocalScales = new Vector3[bodiesToReset.Length];
        _bodyOriginalParents = new Transform[bodiesToReset.Length];

        for (int i = 0; i < bodiesToReset.Length; i++)
        {
            Rigidbody2D rb = bodiesToReset[i];
            if (rb == null)
            {
                continue;
            }

            _bodyLocalPositions[i] = rb.transform.localPosition;
            _bodyLocalRotations[i] = rb.transform.localRotation;
            _bodyLocalScales[i] = rb.transform.localScale;
            _bodyOriginalParents[i] = rb.transform.parent;
        }
    }

    private void RestoreBodyLocalState()
    {
        if (bodiesToReset == null || _bodyLocalPositions == null || _bodyLocalRotations == null || _bodyLocalScales == null || _bodyOriginalParents == null)
        {
            return;
        }

        int count = Mathf.Min(bodiesToReset.Length, _bodyLocalPositions.Length, _bodyLocalRotations.Length, _bodyLocalScales.Length, _bodyOriginalParents.Length);
        for (int i = 0; i < count; i++)
        {
            Rigidbody2D rb = bodiesToReset[i];
            if (rb == null)
            {
                continue;
            }

            Transform originalParent = _bodyOriginalParents[i];
            if (rb.transform.parent != originalParent)
            {
                rb.transform.SetParent(originalParent, false);
            }

            rb.transform.localPosition = _bodyLocalPositions[i];
            rb.transform.localRotation = _bodyLocalRotations[i];
            rb.transform.localScale = _bodyLocalScales[i];
        }
    }

    private void CacheSpringState()
    {
        if ((springsToReset == null || springsToReset.Length == 0) && respawnRoot != null)
        {
            springsToReset = respawnRoot.GetComponentsInChildren<SpringJoint2D>(true);
        }

        if (springsToReset == null)
        {
            _springDistances = null;
            _springFrequencies = null;
            _springDampingRatios = null;
            _springEnabledStates = null;
            return;
        }

        _springDistances = new float[springsToReset.Length];
        _springFrequencies = new float[springsToReset.Length];
        _springDampingRatios = new float[springsToReset.Length];
        _springEnabledStates = new bool[springsToReset.Length];

        for (int i = 0; i < springsToReset.Length; i++)
        {
            SpringJoint2D spring = springsToReset[i];
            if (spring == null)
            {
                continue;
            }

            _springDistances[i] = spring.distance;
            _springFrequencies[i] = spring.frequency;
            _springDampingRatios[i] = spring.dampingRatio;
            _springEnabledStates[i] = spring.enabled;
        }
    }

    private void RestoreSpringState()
    {
        if (springsToReset == null || _springDistances == null || _springFrequencies == null || _springDampingRatios == null || _springEnabledStates == null)
        {
            return;
        }

        int count = Mathf.Min(springsToReset.Length, _springDistances.Length, _springFrequencies.Length, _springDampingRatios.Length, _springEnabledStates.Length);
        for (int i = 0; i < count; i++)
        {
            SpringJoint2D spring = springsToReset[i];
            if (spring == null)
            {
                continue;
            }

            spring.enabled = _springEnabledStates[i];
            spring.distance = _springDistances[i];
            spring.frequency = _springFrequencies[i];
            spring.dampingRatio = _springDampingRatios[i];
        }
    }
}
