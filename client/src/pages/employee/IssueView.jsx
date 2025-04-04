import { useEffect, useState } from "react"
import IssuesList from "../../components/IssuesList"

export default function IssueView() {
    const [issues, setIssues] = useState([])

    async function getIssues() {
        const response = await fetch(`/api/issues`, { credentials: "include" })

        const result = await response.json()

        if (response.ok) {
            setIssues(result.issues)
        } else {
            alert(result.message)
        }
    }

    useEffect(() => {
        getIssues()
        
    }, [])

    return <div className="issuesView">
        <h1>Issues</h1>
        <IssuesList issuesList={issues} getIssues={getIssues} />
    </div>
}
