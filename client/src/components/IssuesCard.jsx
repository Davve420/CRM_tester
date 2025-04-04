import { useState } from "react"

export default function IssuesCard({ issue, getIssues }) {
    const [editState, setEditState] = useState(true)
    const [issueState, setIssueState] = useState(issue.state)
    const created = new Date(issue.created)
    const latest = new Date(issue.latest)

    async function updateState() {
        const response = await fetch(`/api/issues/${issue.id}/state`, {
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
            console.log(result)
            getIssues()
            setEditState(true)
        } else {
            alert(result.message)
        }
    }

    return <div className="issueCard">
        <div className="attributes">
            {
                editState ?
                    <div className="stateColumn">
                        <p>{issue.state}</p>
                        <button className="subjectEditButton" onClick={() => setEditState(!editState)}>&#9998;</button>
                    </div>
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
            <p>{issue.subject}</p>
            <p>{issue.customerEmail}</p>
            <p>{issue.title}</p>
            <p>{latest.toLocaleTimeString("sw-SE")} - {latest.toLocaleDateString("sw-SE")}</p>
            <p>{created.toLocaleTimeString("sw-SE")} - {created.toLocaleDateString("sw-SE")}</p>
            <p>{ }</p>
        </div>
    </div>
}
