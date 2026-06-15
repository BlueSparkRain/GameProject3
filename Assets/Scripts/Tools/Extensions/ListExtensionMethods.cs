using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� List<T> ��ͨ����չ����������
/// ��̬�� + ��̬���� = ��չ�����ı�Ҫ����
/// </summary>
public static class ListExtensionMethods
{
    /// <summary>
    /// ��������չ������������ List<T> �л�ȡһ�����Ԫ��
    /// </summary>
    /// <param name="list">����չ�� List</param>
    /// <typeparam name="T">List ��Ԫ�����ͣ��Զ��ƶϣ������ֶ�ָ����</typeparam>
    /// <returns>���Ԫ�أ��б�Ϊ�շ�������Ĭ��ֵ</returns>
    public static T GetRandomElement<T>(this List<T> list)
    {
        // ��ȫУ�飺��ֹ���б�/�����õ�����Ϸ����
        if (list == null || list.Count == 0)
        {
            DebugManager.LogWarning(EDebugCategory.General, "��ȡ���Ԫ��ʧ�ܣ�List Ϊ null ��գ�");
            return default; // �Զ������������ͣ��������ͷ���null��ֵ���ͷ���0/false��
        }

        // Unity ר���������int ���أ�����ҿ�������ƥ���б�������
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
}