import { useState } from "react"

export default function SubjectCard({ number, subject, getSubjects }) {
    const [edit, setEdit] = useState(true)

    async function updateSubject(formData) {
        const newName = formData.get("newName")
        console.log(newName)
        if (subject.trim().toLowerCase() == newName.trim().toLowerCase()) {
            return alert("No change has been made.")
        }

        const response = await fetch(`/api/forms/subjects`, {
            method: "put",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                oldName: subject,
                newName: newName
            })
        })

        console.log(response.status)

        if (response.ok) {
            getSubjects()
        }
    }

    async function deleteSubject() {
        const response = await fetch(`/api/forms/subjects/${subject}`, {
            method: "delete",
            credentials: "include"
        })

        const result = await response.json()

        if (response.ok) {
            getSubjects()
        } else {
            alert(result.message)
        }
    }

    return <div className="subjectCard">
        <div className="attributes">
            <p>{number + 1}</p>
            {
                !edit ?
                    <form action={updateSubject} className="updateSubjectForm">
                        <input type="text" name="newName" defaultValue={subject} disabled={edit} />
                        <button type="submit" className="subjectUpdateButton">Save</button>
                    </form>
                    :
                    <p>{subject}</p>
            }
            <div className="subjectButtons">
                <button className="subjectEditButton" onClick={() => setEdit(!edit)}>&#9998;</button>
                <button className="removeButton" onClick={deleteSubject}>&#10006;</button>
            </div>
        </div>
    </div>
}
