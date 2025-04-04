import { useEffect, useState } from "react"
import SubjectList from "../../components/SubjectList.jsx"

export default function EditForm() {
    const [subjects, setSubjects] = useState([])
    const [showAddForm, setShowAddForm] = useState(false)

    async function getSubjects() {
        const response = await fetch(`/api/forms/subjects`, { credentials: "include" })
        const results = await response.json()

        console.log(results)

        if (response.ok) {
            setSubjects(results.subjects)
        } else {
            alert(result.message)
        }
    }

    async function addSubject(formData) {
        const response = await fetch(`/api/forms/subjects`, {
            method: "post",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                name: formData.get("newSubject")
            })
        })
        const result = await response.json()

        if (response.ok) {
            getSubjects()
        } else {
            console.log(result)
            alert(result.detail)
        }
    }

    useEffect(() => {
        getSubjects()
    }, [])

    return <div className="subjectView">
        <h1>Form Subjects</h1>
        <div id="subjectViewMenu">
            <button onClick={() => setShowAddForm(!showAddForm)}>New Subject</button>
        </div>
        {
            showAddForm ?
                <form action={addSubject}>
                    <input type="text" name="newSubject" placeholder="New Subject" required />
                    <button type="submit">Save</button>
                </form>
                :
                null
        }
        <SubjectList subjectList={subjects} getSubjects={getSubjects} />
    </div>
}
