using UnityEngine;

// 确保您的场景中有一个激活的 Monkey 脚本
[RequireComponent(typeof(Monkey))]
public class MonkeyAutoMove_Test : MonoBehaviour
{
    // 对 Monkey 脚本的引用
    private Monkey monkey;

    // 一个标志位，防止在猴子移动过程中重复发送指令
    private bool isMonkeyBusy = false;

    private void Awake()
    {
        // 在游戏开始时自动获取同一个游戏对象上的 Monkey 脚本
        monkey = GetComponent<Monkey>();
    }

    private void Update()
    {
        // 如果isMonkeyBusy为true，说明猴子正在执行一个自动移动任务，此时不接受新指令
        if (isMonkeyBusy)
        {
            return;
        }

        // --- 测试指令 ---

        // 当按下 "K" 键
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("指令：开始向右移动 1 个单位...");
            isMonkeyBusy = true; // 标记猴子开始忙碌

            // 调用猴子的移动函数
            monkey.MoveHorizontally(1.0f, (result) => 
            {
                // 当移动完成时，这个部分的代码（回调函数）会被执行
                if (result.Succeeded)
                {
                    Debug.Log("测试成功: " + result.Message);
                }
                else
                {
                    Debug.Log("测试失败: " + result.Message);
                }
                isMonkeyBusy = false; // 任务结束，解除忙碌状态
            });
        }

        // 当按下 "J" 键
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("指令：开始向左移动 1 个单位...");
            isMonkeyBusy = true; // 标记猴子开始忙碌

            // 调用猴子的移动函数
            monkey.MoveHorizontally(-1.0f, (result) => 
            {
                // 当移动完成时，这个部分的代码（回调函数）会被执行
                if (result.Succeeded)
                {
                    Debug.Log("测试成功: " + result.Message);
                }
                else
                {
                    Debug.Log("测试失败: " + result.Message);
                }
                isMonkeyBusy = false; // 任务结束, 解除忙碌状态
            });
        }
    }
}