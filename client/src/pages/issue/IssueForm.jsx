import { useEffect, useState } from "react"
import { useParams } from "react-router"

export default function IssueForm() {
    const [formInfo, setFormInfo] = useState("")
    const [subject, setSubject] = useState("")
    const [message, setMessage] = useState("")
    const { company_name } = useParams()

    async function createIssue(formData) {
        const response = await fetch(`/api/issues/create/${company_name}`, {
            method: "post",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email: formData.get("email"),
                title: formData.get("title"),
                subject: subject,
                message: message
            })
        })

        const result = await response.json()

        if (response.ok) {
            alert(`Thanks for your issue, you will get a confirmation sent to ${formData.get("email")}.`)
        } else {
            alert(result.message)
        }
    }

    async function getCompanyForm() {
        const response = await fetch(`/api/forms/${company_name}`, { credentials: "include" })
        const result = await response.json()

        console.log("Company Form: ", result)
        if (response.ok) {
            setFormInfo(result.company_info)
            setSubject(result.company_info.subjects[0])
        } else {
            setFormInfo(null)
        }
    }

    useEffect(() => {
        getCompanyForm()
    }, [])

    return !formInfo ?
        <h1>No Company was found!</h1>
        :
        <div >
            <h1>{formInfo.companyName} issue form.</h1>
            <form action={createIssue}>
                <label>
                    Your email
                    <input type="email" name="email" required />
                </label>
                <label>
                    Title
                    <input type="text" name="title" minLength={3} required />
                </label>
                <label>
                    Subject
                    <select defaultValue={formInfo.subjects[0]} onChange={e => setSubject(e.target.value)}>
                        {
                            formInfo.subjects.map(subject => <option key={subject} value={subject}>{subject}</option>)
                        }
                    </select>
                </label>
                <label>
                    Message
                    <textarea name="message"
                        onChange={e => setMessage(e.target.value)}
                        rows={8}
                        cols={50}
                        placeholder="Describe your issue."
                        maxLength={1000}
                        minLength={10}
                        wrap="hard"
                        required />
                </label>
                <button type="submit">Create issue</button>
            </form>
        </div>
}
