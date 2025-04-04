import SubjectCard from "./SubjectCard.jsx";

export default function SubjectList({ subjectList, getSubjects }) {
    return <div className="list">
        <div className="subjectListHeaders">
            <p>Number</p>
            <p>Name</p>
            <p>Edit / Delete</p>
        </div>
        {subjectList.map((subject, index) => <SubjectCard key={subject + index} number={index} subject={subject} getSubjects={getSubjects} />)}
    </div>
}