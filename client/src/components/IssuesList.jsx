import IssuesCard from "./IssuesCard.jsx";

export default function IssuesList({ issuesList, getIssues }) {
    return <div className="list">
        <div className="issuesListHeaders">
            <p>State</p>
            <p>Subject</p>
            <p>Customer email</p>
            <p>Title</p>
            <p>Latest Update</p>
            <p>Created</p>
            <p>Employee</p>
        </div>
        {issuesList.map(issue => <IssuesCard key={issue.id} issue={issue} getIssues={getIssues} />)}
    </div>
}
