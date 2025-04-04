import { useContext, useEffect, useState } from "react"
import { useParams } from "react-router"
import Chat from "../../components/Chat.jsx"
import { GlobalContext } from "../../GlobalContext.jsx"

export default function EmployeeChat() {
    const { user } = useContext(GlobalContext)
    const { issue_id } = useParams()
    const [editState, setEditState] = useState(true)
    const [issueState, setIssueState] = useState(null)
    const [issue, setIssue] = useState(null)
    const [messages, setMessages] = useState([])

    async function updateState() {
        const response = await fetch(`/api/issues/${issue_id}/state`, {
            method: "put",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                oldState: issue.state,
                newState: issueState
            })
        })

        const result = await response.json()

        if (response.ok) {
            getIssueInfo()
            setEditState(true)
        } else {
            alert(result.message)
        }
    }

    async function getIssueInfo() {
        const response = await fetch(`/api/issues/${issue_id}`, { credentials: "include" })
        const result = await response.json()

        if (response.ok) {
            setIssue(result)
            setIssueState(result.state)
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
                {
                    editState ?
                        <button className="subjectEditButton" onClick={() => setEditState(!editState)}>State: {issueState}</button>
                        :
                        <form action={updateState} className="stateForm">
                            <select defaultValue={issue.state} onChange={e => setIssueState(e.target.value)} className="stateSelect">
                                <option value={'NEW'}>NEW</option>
                                <option value={'OPEN'}>OPEN</option>
                                <option value={'CLOSED'}>CLOSED</option>
                            </select>
                            <div className="stateButtons">
                                <button type="submit" className="stateUpdateButton">Save</button>
                                <button type="button" className="removeButton" onClick={() => setEditState(!editState)}>&#10006;</button>
                            </div>
                        </form>
                }
            </div>
            <Chat messages={messages} issue_id={issue_id} getMessages={getMessages} customerSupport={true} username={user.username} role={"SUPPORT"} />
        </div>
}


