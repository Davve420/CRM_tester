import { useEffect, useState } from "react"
import { useParams } from "react-router"
import Chat from "../../components/Chat.jsx"

export default function CustomerChat() {
    const { issue_id } = useParams()
    const [issue, setIssue] = useState(null)
    const [messages, setMessages] = useState([])

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
                username: issue.customerEmail
            })
        })

        const result = await response.json()

        if (response.ok) {
            getMessages()
        } else {
            alert(result.message)
        }
    }

    async function getIssueInfo() {
        const response = await fetch(`/api/issues/${issue_id}`, { credentials: "include" })
        const result = await response.json()

        if (response.ok) {
            setIssue(result)
            getMessages()
        }

    }

    async function getMessages() {
        const response = await fetch(`/api/issues/${issue_id}/messages`, { credentials: "include" })
        const result = await response.json()

        if (response.ok) {
            setMessages(result.messages)
        }
    }

    useEffect(() => {
        getIssueInfo()
    }, [])

    return !issue ?
        <h1>No issue was found</h1>
        :
        <div id="issueChat">
            <h1>{issue.companyName} - Chat</h1>
            <div id="issueInfo">
                <p id="issueTitle">Title: {issue.title}</p>
                <p id="issueState">State: {issue.state}</p>
            </div>
            <Chat messages={messages} issue_id={issue_id} getMessages={getMessages} customerSupport={false} username={issue.customerEmail} role={"CUSTOMER"} />
        </div>
}


