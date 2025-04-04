import { useEffect } from "react"

export default function Chat({ messages, issue_id, getMessages, customerSupport, username, role }) {

    function isReceiver(sender) {
        if (sender == "CUSTOMER") {
            return customerSupport ? true : false
        } else {
            return customerSupport ? false : true
        }
    }

    async function sendMessage(formData) {
        const message = formData.get("message")
        if (message.trim().length == 0) {
            return alert("Message is to short.")
        }

        const response = await fetch(`/api/issues/${issue_id}/messages`, {
            method: "post",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                message: message,
                username: username,
                sender: role
            })
        })

        const result = await response.json()

        if (response.ok) {
            getMessages()
        } else {
            alert(result.message)
        }
    }

    useEffect(() => {
        function onTimeout() {
            getMessages()
        }

        const timeoutId = setTimeout(onTimeout, 2000);

        return () => {
            clearTimeout(timeoutId);
        };
    }, [messages])

    return <div id="chat">
        <div id="messageList">
            {
                messages.map((message, index) => <div key={index} className={"message" + (isReceiver(message.sender) ? " receiver" : "")}>
                    <p className="messageText">{message.text}</p>
                    <p className="messageInfo"><span className="messageUsername">{message.username}</span> - <span className="messageTime">10 mars 2025 23:12</span></p>
                </div>)
            }
        </div>
        <form action={sendMessage} id="sendMessage">
            <input type="text" name="message" placeholder="Type message" required />
            <button type="submit">Send</button>
        </form>
    </div>
}
