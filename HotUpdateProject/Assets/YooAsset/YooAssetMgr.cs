// YooAssetMgr.cs
// MomoUnity
// 
// Created by Shen Hua on 02/06/2026
// Copyright (c) 2026 Momo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

namespace YooAsset
{
    public class YooAssetMgr : MonoBehaviour
    {
        private void Start()
        {
            YooAssets.Initialize();
            var package = YooAssets.CreatePackage("DefaultPackage");
            YooAssets.SetDefaultPackage(package);
            StartCoroutine(InitPackage(package));
        }

        private IEnumerator InitPackage(ResourcePackage package)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;

            var initOperation = package.InitializeAsync(createParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
                var opReqVersion = package.RequestPackageVersionAsync();
                yield return opReqVersion;
                if (opReqVersion.Status == EOperationStatus.Succeed)
                {
                    var opUpdateManifest = package.UpdatePackageManifestAsync(opReqVersion.PackageVersion);
                    yield return opUpdateManifest;
                    if (opUpdateManifest.Status == EOperationStatus.Succeed)
                        StartCoroutine(Load(package));
                }
            }
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

        private IEnumerator DestroyPackage()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            DestroyOperation operation = package.DestroyAsync();
            yield return operation;

            if (YooAssets.RemovePackage(package))
            {
                Debug.Log("移除成功！");
            }
        }

        IEnumerator Load(ResourcePackage package)
        {
            AssetHandle handle = package.LoadAssetAsync<GameObject>("Assets/YooAsset/Res/YooTest.prefab");
            yield return handle;
            GameObject go = handle.InstantiateSync();
            Debug.Log($"Prefab name is {go.name}");
        }
    }
}